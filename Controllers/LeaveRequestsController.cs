using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutOfOffice.Data;
using OutOfOffice.Models;

namespace OutOfOffice.Controllers
{
	[Route("leavereq")]
	public class LeaveRequestsController : Controller
	{
		private readonly AppDBContext _dbContext;

		public LeaveRequestsController(AppDBContext dbContext)
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

		// GET: leavereq/index
		[Route("index")]
		public IActionResult Index()
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
				case 3:
					var leaveRequests = _dbContext.LeaveRequests.Include(e => e.Employee);
					if (leaveRequests.Any())
					{
						return View(leaveRequests.ToList());
					}
					return View();
				default:
					var empLeaveReqests = _dbContext.LeaveRequests.Include(e => e.Employee).Where(x => x.EmployeeID == currentUser.ID);
					if (empLeaveReqests.Any())
					{
						return View("EmployeeIndex", empLeaveReqests.ToList());
					}
					return View("EmployeeIndex");
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

			var requests = _dbContext.LeaveRequests.Include(e => e.Employee).Where(x => x.ID.ToString() == searchQuery);

			switch (currentUser.Role)
			{
				case 1:
				case 2:
					if (requests.Any())
					{
						return View("Index", requests.ToList());
					}
					else
					{
						requests = _dbContext.LeaveRequests.Include(e => e.Employee);
						if (requests.Any())
						{
							return View("Index", requests.ToList());
						}
						return View("Index");
					}
				case 3:
					if (requests.Any())
					{
						return View("Index", requests.ToList());
					}
					else
					{
						requests = _dbContext.LeaveRequests.Include(e => e.Employee);
						if (requests.Any())
						{
							return View("PMIndex", requests.ToList());
						}
						return View("PMIndex");
					}
				default:
					if (requests.Any())
					{
						return View("EmployeeIndex", requests.ToList());
					}
					else
					{
						requests = _dbContext.LeaveRequests.Include(e => e.Employee);
						if (requests.Any())
						{
							return View("EmployeeIndex", requests.ToList());
						}
						return View("Index");
					}
			}
		}

		// POST: leavereq/create
		[HttpPost]
		[Route("create")]
		public IActionResult Create(LeaveReq leaveRequest)
		{
			var employee = _dbContext.Employees.FirstOrDefault(x => x.ID == leaveRequest.EmployeeID);
			if (employee != null)
			{
				leaveRequest.Employee = employee;
				employee.LeaveRequests.Add(leaveRequest);
			}

			if (leaveRequest.EndDate < leaveRequest.StartDate)
			{
				ViewBag.currentUser = GetCurrentUser();
				return View("CreateForm");
			}

			_dbContext.Add(leaveRequest);

			_dbContext.SaveChanges();

			return RedirectToAction("Index");
		}

		// GET: leavereq/createform
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
				case 2:
				case 3:
					return View("NoAccess");
				default:
					ViewBag.currentUser = currentUser;
					return View("CreateForm");
			}
		}

		// GET: leavereq/editform/id
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
				case 2:
				case 3:
					return View("NoAccess");
				default:
					var leaveRequest = _dbContext.LeaveRequests.FirstOrDefault(x => x.ID == id);
					if (leaveRequest == null)
					{
						return NotFound();
					}
					if (currentUser.ID == leaveRequest.EmployeeID)
					{
						ViewBag.currentUser = currentUser;
						return View("EditForm", leaveRequest);
					}
					return View("NoAccess");
			}
		}

		// POST: leavereq/edit
		[HttpPost]
		[Route("edit")]
		public IActionResult Edit(LeaveReq leaveRequest)
		{
			if (leaveRequest.EndDate < leaveRequest.StartDate)
			{
				ViewBag.currentUser = GetCurrentUser();
				return View("EditForm", leaveRequest);
			}

			var existingLeaveReq = _dbContext.LeaveRequests.FirstOrDefault(x => x.ID == leaveRequest.ID);
			if (existingLeaveReq != null)
			{
				existingLeaveReq.EmployeeID = leaveRequest.EmployeeID;
				existingLeaveReq.AbsenceReason = leaveRequest.AbsenceReason;
				existingLeaveReq.StartDate = leaveRequest.StartDate;
				existingLeaveReq.EndDate = leaveRequest.EndDate;
				existingLeaveReq.Comment = leaveRequest.Comment;
				existingLeaveReq.Status = leaveRequest.Status;
				if (leaveRequest.EmployeeID != null)
				{
					var employee = _dbContext.Employees.FirstOrDefault(x => x.ID == leaveRequest.EmployeeID);
					if (employee != null)
					{
						existingLeaveReq.Employee = employee;
					}
				}

				_dbContext.SaveChanges();

				if (existingLeaveReq.Status == "Submitted")
				{
					var currentUser = GetCurrentUser();

					var projects = _dbContext.Projects
						.Where(p => p.ProjectManagerID == currentUser.ID ||
									p.ProjectEmployees.Any(pe => pe.EmployeeID == currentUser.ID))
						.ToList();

					foreach (var project in projects)
					{
						var approvers = new List<Employee>();

						var projectManager = _dbContext.Employees.FirstOrDefault(e => e.ID == project.ProjectManagerID);
						if (projectManager != null)
						{
							approvers.Add(projectManager);
						}

						if (currentUser.PeoplePartnerID.HasValue)
						{
							var peoplePartner = _dbContext.Employees.FirstOrDefault(e => e.ID == currentUser.PeoplePartnerID.Value);
							if (peoplePartner != null)
							{
								approvers.Add(peoplePartner);
							}
						}

						foreach (var approver in approvers)
						{
							var approvalRequest = new ApprovalReq
							{
								ApproverID = approver.ID,
								LeaveRequestID = leaveRequest.ID,
								Status = "Pending",
								Comment = null
							};

							_dbContext.Add(approvalRequest);
						}
					}

					_dbContext.SaveChanges();
				}

				else if (leaveRequest.Status == "Disabled")
				{
					var approvalRequests = _dbContext.ApprovalRequests
						.Where(ar => ar.LeaveRequestID == leaveRequest.ID)
						.ToList();

					_dbContext.ApprovalRequests.RemoveRange(approvalRequests);
					_dbContext.SaveChanges();
				}

				return RedirectToAction("Index");
			}
			else
			{
				return NotFound();
			}
		}

		// GET: leavereq/delete/id
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
				case 3:
					return View("NoAccess");
				default:
					var existingLeaveReq = _dbContext.LeaveRequests.FirstOrDefault(x => x.ID == id);

					if (existingLeaveReq == null)
					{
						return NotFound();
					}

					if (currentUser.ID == existingLeaveReq.EmployeeID)
					{
						return View("DeleteForm", existingLeaveReq);
					}

					return View("NoAccess");
			}
		}

		// POST: leavereq/delete
		[HttpPost]
		[Route("delete")]
		public IActionResult Delete(int id)
		{
			var leaveRequest = _dbContext.LeaveRequests.FirstOrDefault(x => x.ID == id);
			if (leaveRequest != null)
			{
				var approvalRequests = _dbContext.ApprovalRequests
						.Where(ar => ar.LeaveRequestID == leaveRequest.ID)
						.ToList();

				_dbContext.ApprovalRequests.RemoveRange(approvalRequests);

				_dbContext.LeaveRequests.Remove(leaveRequest);

				_dbContext.SaveChanges();

				return RedirectToAction("Index");
			}
			return NotFound();
		}

		// GET: leavereq/details/id
		[HttpGet]
		[Route("details/{id}")]
		public IActionResult Details(int id)
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			var leaveRequest = _dbContext.LeaveRequests.Include(e => e.Employee).FirstOrDefault(x => x.ID == id);
			if (leaveRequest == null)
			{
				return NotFound();
			}

			switch (currentUser.Role)
			{
				case 1:
				case 2:
				case 3:
					return View("Details", leaveRequest);
				default:
					return View("EmployeeDetails", leaveRequest);
			}
		}
	}
}
