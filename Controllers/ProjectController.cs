using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutOfOffice.Data;
using OutOfOffice.Models;

namespace OutOfOffice.Controllers
{
	[Route("project")]
	public class ProjectController : Controller
	{
		private readonly AppDBContext _dbContext;

		public ProjectController(AppDBContext dbContext)
		{
			_dbContext = dbContext;
		}

		private Employee GetCurrentUser()
		{
			string loggedUserId = HttpContext.Session.GetString("LoggedUserID");
			if (loggedUserId == null)
			{
				return null;
			}

			if (!int.TryParse(loggedUserId, out int userId))
			{
				return null;
			}

			var currentUser = _dbContext.Employees.FirstOrDefault(x => x.ID == userId);
			return currentUser;
		}

		// GET: project/index
		[Route("index")]
		public IActionResult Index()
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			var projects = _dbContext.Projects.Include(e => e.ProjectManager).ToList();
			switch (currentUser.Role)
			{
				case 1:
				case 3:
					return View(projects);
				case 2:
					return View("HRIndex", projects);
				default:
					return View("NoAccess");
			}
		}

		[HttpPost]
		[Route("search")]
		public IActionResult Search(string searchQuery)
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			var projects = _dbContext.Projects.Include(e => e.ProjectManager).Where(x => x.ID.ToString() == searchQuery);

			switch (currentUser.Role)
			{
				case 1:
				case 3:
					if (projects.Any())
					{
						return View("Index", projects.ToList());
					}
					else
					{
						projects = _dbContext.Projects.Include(e => e.ProjectManager);
						if (projects.Any())
						{
							return View("Index", projects.ToList());
						}
						return View("Index");
					}
				case 2:
					if (projects.Any())
					{
						return View("Index", projects.ToList());
					}
					else
					{
						projects = _dbContext.Projects.Include(e => e.ProjectManager);
						if (projects.Any())
						{
							return View("HRIndex", projects.ToList());
						}
						return View("HRIndex");
					}
				default:
					return View("NoAccess");
			}
		}

		// GET: project/details/id
		[HttpGet]
		[Route("details/{id}")]
		public IActionResult Details(int id)
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			var project = _dbContext.Projects.Include(e => e.ProjectManager).FirstOrDefault(x => x.ID == id);
			switch (currentUser.Role)
			{
				case 1:
				case 3:
					return View(project);
				case 2:
					return View("HRDetails", project);
				default:
					return View("NoAccess");
			}
		}

		// GET: project/createform
		[HttpGet]
		[Route("createform")]
		public IActionResult CreateForm()
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			switch (currentUser.Role)
			{
				case 1:
				case 3:
					var managers = _dbContext.Employees.Where(e => e.Position == "Manager"
																	|| e.Position == "manager"
																	|| e.Position == "PM");
					if (managers != null)
					{
						ViewBag.managers = managers.ToList();
					}
					return View("CreateForm");
				default: return View("NotFound");
			}
		}

		// POST: project/create
		[HttpPost]
		[Route("create")]
		public IActionResult Create(Project project)
		{
			if (project.ProjectManagerID != null)
			{
				var manager = _dbContext.Employees.FirstOrDefault(x => x.ID == project.ProjectManagerID);

				if (manager != null)
				{
					project.ProjectManager = manager;
					manager.ManagedProjects.Add(project);
				}
			}

			_dbContext.Add(project);

			_dbContext.SaveChanges();

			return RedirectToAction("Index");
		}

		// GET: project/editform/id
		[HttpGet]
		[Route("editform/{id}")]
		public IActionResult EditForm(int id)
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			switch (currentUser.Role)
			{
				case 1:
				case 3:
					var managers = _dbContext.Employees.Where(e => e.Position == "Manager"
																	|| e.Position == "manager"
																	|| e.Position == "PM");
					if (managers != null)
					{
						ViewBag.managers = managers.ToList();
					}
					var project = _dbContext.Projects.FirstOrDefault(x => x.ID == id);
					if (project != null)
					{
						return View("EditForm", project);
					}
					return NotFound();
				default:
					return View("NoAccess");
			}


		}

		// POST: project/edit
		[HttpPost]
		[Route("edit")]
		public IActionResult Edit(Project project)
		{
			var existingProject = _dbContext.Projects.FirstOrDefault(x => x.ID == project.ID);
			if (existingProject != null)
			{
				if (existingProject.ProjectManagerID != null)
				{
					var manager = _dbContext.Employees.FirstOrDefault(x => x.ID == project.ProjectManagerID);
					if (manager != null)
					{
						existingProject.ProjectManager = manager;
					}
				}
				existingProject.ProjectType = project.ProjectType;
				existingProject.StartDate = project.StartDate;
				existingProject.EndDate = project.EndDate;
				existingProject.Comment = project.Comment;
				existingProject.Status = project.Status;

				_dbContext.SaveChanges();

				return RedirectToAction("Index");
			}
			else
			{
				return NotFound();
			}
		}

		// GET: project/deleteform/id
		[HttpGet]
		[Route("deleteform/{id}")]
		public IActionResult DeleteForm(int id)
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			switch (currentUser.Role)
			{
				case 1:
				case 3:
					var project = _dbContext.Projects.FirstOrDefault(x => x.ID == id);
					if (project != null)
					{
						return View("DeleteForm", project);
					}
					return NotFound();
				default:
					return View("NoAccess");
			}

		}

		// POST: project/delete
		[HttpPost]
		[Route("delete")]
		public IActionResult Delete(int id)
		{
			var project = _dbContext.Projects.FirstOrDefault(x => x.ID == id);
			if (project != null)
			{
				_dbContext.Projects.Remove(project);

				_dbContext.SaveChanges();

				return RedirectToAction("Index");
			}
			return NotFound();
		}
	}
}
