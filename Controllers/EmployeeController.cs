using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OutOfOffice.Data;
using OutOfOffice.Models;

namespace OutOfOffice.Controllers
{
	[Route("employee")]
	public class EmployeeController : Controller
	{
		private readonly AppDBContext _dbContext;

		public EmployeeController(AppDBContext dbContext)
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

		// GET: employee/index
		[Route("index")]
		public IActionResult Index()
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			var employees = _dbContext.Employees;

			switch (currentUser.Role)
			{
				case 1:
				case 2:

					if (employees.Any())
					{
						return View(employees.ToList());
					}
					return View();
				case 3:
					if (employees.Any())
					{
						return View("PMIndex", employees.ToList());
					}
					return View();
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

			var employees = _dbContext.Employees.Where(x => x.FullName.Contains(searchQuery));

			switch (currentUser.Role)
			{
				case 1:
				case 2:
					if (employees.Any())
					{
						return View("Index", employees.ToList());
					}
					else
					{
						employees = _dbContext.Employees;
						return View("Index", employees.ToList());
					}
				case 3:
					if (employees.Any())
					{
						return View("PMIndex", employees.ToList());
					}
					else
					{
						employees = _dbContext.Employees;
						return View("PMIndex", employees.ToList());
					}
				default:
					return View("NoAccess");
			}
		}

		// POST: employee/create
		[HttpPost]
		[Route("create")]
		public IActionResult Create(Employee employee, IFormFile Photo)
		{
			bool existingLogin = _dbContext.Employees.Any(x => x.Login == employee.Login);
			if (existingLogin)
			{
				return RedirectToAction("CreateForm");
			}

			if (Photo != null && Photo.Length > 0)
			{
				using (var memoryStream = new MemoryStream())
				{
					Photo.CopyTo(memoryStream);
					employee.Photo = memoryStream.ToArray();
				}
			}

			if (employee.PeoplePartnerID != null)
			{
				var partner = _dbContext.Employees.FirstOrDefault(x => x.ID == employee.PeoplePartnerID);
				if (partner != null)
				{
					employee.PeoplePartner = partner;
				}
			}

			_dbContext.Add(employee);

			_dbContext.SaveChanges();

			return RedirectToAction("Index");
		}

		// GET: employee/createform
		[HttpGet]
		[Route("createform")]
		public IActionResult CreateForm()
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			var partners = _dbContext.Employees.Where(e => e.Position == "HR" || e.Position == "hr" || e.Position == "Hr");

			switch (currentUser.Role)
			{
				case 1:
					if (partners != null)
					{
						ViewBag.partners = partners.ToList();
					}
					return View("CreateForm");
				case 2:
					if (partners != null)
					{
						ViewBag.partners = partners.ToList();
					}
					return View("HRCreateForm");
				default:
					return View("NoAccess");
			}
		}

		// GET: employee/editform/id
		[HttpGet]
		[Route("editform/{id}")]
		public IActionResult EditForm(int id)
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			var employee = _dbContext.Employees.FirstOrDefault(x => x.ID == id);
			if (employee == null)
			{
				return NotFound();
			}

			var partners = _dbContext.Employees.Where(e => e.Position == "HR" || e.Position == "hr" || e.Position == "Hr");

			switch (currentUser.Role)
			{
				case 1:
					if (partners != null)
					{
						ViewBag.partners = partners.ToList();
					}
					return View("EditForm", employee);
				case 2:
					if (partners != null)
					{
						ViewBag.partners = partners.ToList();
					}
					return View("HREditForm", employee);
				case 3:
					var pmProjects = _dbContext.Projects
						.Where(x => x.ProjectManagerID == currentUser.ID &&
						!x.ProjectEmployees.Any(pe => pe.EmployeeID == currentUser.ID));

					if (pmProjects.Any())
					{
						ViewBag.pmProjects = pmProjects.ToList();
					}
					return View("PMEditForm", employee);
				default:
					return View("NoAccess");
			}
		}

		// POST: employee/edit
		[HttpPost]
		[Route("edit")]
		public IActionResult Edit(Employee employee, IFormFile Photo)
		{
			var existingEmployee = _dbContext.Employees.FirstOrDefault(x => x.ID == employee.ID);
			if (existingEmployee != null)
			{
				existingEmployee.FullName = employee.FullName;
				existingEmployee.Subdivision = employee.Subdivision;
				existingEmployee.Position = employee.Position;
				existingEmployee.Status = employee.Status;
				existingEmployee.PeoplePartnerID = employee.PeoplePartnerID;
				existingEmployee.Balance = employee.Balance;
				existingEmployee.Password = employee.Password;
				existingEmployee.Login = employee.Login;
				existingEmployee.Role = employee.Role;

				if (Photo != null && Photo.Length > 0)
				{
					using (var memoryStream = new MemoryStream())
					{
						Photo.CopyTo(memoryStream);
						existingEmployee.Photo = memoryStream.ToArray();
					}
				}
				if (employee.PeoplePartnerID != null)
				{
					var partner = _dbContext.Employees.FirstOrDefault(x => x.ID == employee.PeoplePartnerID);
					if (partner != null)
					{
						existingEmployee.PeoplePartner = partner;
					}
				}

				_dbContext.SaveChanges();

				return RedirectToAction("Index");
			}
			else
			{
				return NotFound();
			}
		}

		// POST: employee/pmedit
		[HttpPost]
		[Route("pmedit")]
		public IActionResult PMEdit(int employeeID, int projectID)
		{
			var employee = _dbContext.Employees
	.Include(e => e.ProjectEmployees)
	.FirstOrDefault(e => e.ID == employeeID);

			if (employee != null)
			{
				var projectToAdd = _dbContext.Projects.FirstOrDefault(p => p.ID == projectID);

				if (projectToAdd != null)
				{
					if (!employee.ProjectEmployees.Any(pe => pe.EmployeeID == employeeID && pe.ProjectID == projectID))
					{
						var newProjectEmployee = new ProjectEmployee
						{
							EmployeeID = employeeID,
							ProjectID = projectID,
							Employee = employee,
							Project = projectToAdd
						};

						employee.ProjectEmployees.Add(newProjectEmployee);

						_dbContext.SaveChanges();

						return RedirectToAction("Index");
					}
				}
			}

			return View("Error");

		}

		// GET: employee/delete/id
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
				case 2:
					var exitstingEmployee = _dbContext.Employees.FirstOrDefault(x => x.ID == id);
					if (exitstingEmployee != null)
					{
						return View("DeleteForm", exitstingEmployee);
					}

					return NotFound();
				default:
					return View("NoAccess");
			}
		}

		// POST: employee/delete
		[HttpPost]
		[Route("delete")]
		public IActionResult Delete(int id)
		{
			var employee = _dbContext.Employees.FirstOrDefault(x => x.ID == id);
			if (employee != null)
			{
				_dbContext.Employees.Remove(employee);

				_dbContext.SaveChanges();

				return RedirectToAction("Index");
			}
			return NotFound();
		}

		// GET: employee/details/id
		[HttpGet]
		[Route("details/{id}")]
		public IActionResult Details(int id)
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			var employee = _dbContext.Employees
								.Include(e => e.PeoplePartner)
								.Include(e => e.LeaveRequests)
								.Include(e => e.ManagedProjects)
								.Include(e => e.ProjectEmployees)
								.ThenInclude(pe => pe.Project)
								.FirstOrDefault(x => x.ID == id);

			switch (currentUser.Role)
			{
				case 1:
				case 2:
					if (employee != null)
					{
						return View(employee);
					}
					return NotFound();
				case 3:
					if (employee != null)
					{
						return View("PMDetails", employee);
					}
					return NotFound();
				default:
					return View("NoAccess");
			}
		}

		[HttpGet]
		[Route("login")]
		public IActionResult LoginForm()
		{
			return View("LoginForm");
		}

		[HttpPost]
		[Route("loginconfirm")]
		public IActionResult Login(string login, string password)
		{
			var employee = _dbContext.Employees.FirstOrDefault(x => x.Login == login && x.Password == password);
			if (employee != null)
			{
				HttpContext.Session.SetString("LoggedUserID", employee.ID.ToString());
				return Redirect("/");
			}
			return View("LoginForm");
		}
	}
}
