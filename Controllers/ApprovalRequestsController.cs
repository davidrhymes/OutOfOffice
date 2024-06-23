using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OutOfOffice.Data;
using OutOfOffice.Models;

namespace OutOfOffice.Controllers
{
	[Route("approvalreq")]
	public class ApprovalRequestsController : Controller
	{
		private readonly AppDBContext _dbContext;

		public ApprovalRequestsController(AppDBContext dbContext)
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

		// GET: approvalreq/index
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
					var requests = _dbContext.ApprovalRequests.Include(e => e.Approver).Include(e => e.LeaveRequest);
					if (requests.Any())
					{
						return View(requests.ToList());
					}
					return View();
				case 2:
				case 3:
					var userApprReq = _dbContext.ApprovalRequests
						.Include(e => e.Approver)
						.Include(e => e.LeaveRequest)
						.Where(ar => ar.ApproverID == currentUser.ID);
					if (userApprReq.Any())
					{
						return View(userApprReq.ToList());
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
			var requests = _dbContext.ApprovalRequests.Include(a => a.Approver).Include(lr => lr.LeaveRequest).Where(x => x.ID.ToString() == searchQuery);
			if (requests.Any())
			{
				return View("Index", requests.ToList());
			}
			else
			{
				requests = _dbContext.ApprovalRequests.Include(e => e.Approver).Include(e => e.LeaveRequest);
				return View("Index", requests.ToList());
			}
		}

		// GET: approvalreq/details/id
		[HttpGet]
		[Route("details/{id}")]
		public IActionResult Details(int id)
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null)
			{
				return View("NoAccess");
			}

			switch(currentUser.Role)
			{
				case 1:
				case 2:
				case 3:
					var request = _dbContext.ApprovalRequests.Include(e => e.Approver).Include(e => e.LeaveRequest).FirstOrDefault(x => x.ID == id);
					if (request == null)
					{
						return NotFound();
					}
					if (request.ApproverID == currentUser.ID || currentUser.Role == 1)
					{
						return View(request);
					}
					return View("NoAccess");
				default:
					return View("NoAccess");
			}
		}

		// GET: approvalreq/createform
		//[HttpGet]
		//[Route("createform")]
		//public IActionResult CreateForm()
		//{
		//	return View("CreateForm");
		//}

		//// POST: approvalreq/create
		//[HttpPost]
		//[Route("create")]
		//public IActionResult Create(ApprovalReq approvalRequest)
		//{
		//	if (approvalRequest.ApproverID != null && approvalRequest.LeaveRequestID != null)
		//	{
		//		var approver = _dbContext.Employees.FirstOrDefault(x => x.ID ==  approvalRequest.ApproverID);
		//		var request = _dbContext.LeaveRequests.FirstOrDefault(x => x.ID == approvalRequest.LeaveRequestID);
		//		if (approver != null && request != null)
		//		{
		//			approvalRequest.Approver = approver;
		//			approvalRequest.LeaveRequest = request;
		//		}
		//	}

		//	_dbContext.Add(approvalRequest);

		//	_dbContext.SaveChanges();

		//	if (approvalRequest.Status == "Approved")
		//	{
		//		var otherApprovalRequests = _dbContext.ApprovalRequests
		//				.Where(ar => ar.LeaveRequestID == approvalRequest.LeaveRequestID && ar.ID != approvalRequest.ID)
		//				.ToList();

		//		_dbContext.ApprovalRequests.RemoveRange(otherApprovalRequests);

		//		approvalRequest.LeaveRequest.Status = "Approved";
		//		approvalRequest.LeaveRequest.Comment = approvalRequest.Comment;
		//	}
		//	if (approvalRequest.Status == "Declined")
		//	{
		//		approvalRequest.LeaveRequest.Status = "Declined";
		//		approvalRequest.LeaveRequest.Comment = approvalRequest.Comment;
		//	}

		//	_dbContext.SaveChanges();

		//	return RedirectToAction("Index");
		//}

		// GET: approvalreq/editform/id
		[HttpGet]
		[Route("editform/{id}")]
		public IActionResult EditForm(int id)
		{
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return View("NoAccess");
            }

			switch(currentUser.Role)
			{
				case 1:
				case 2:
				case 3:
                    var request = _dbContext.ApprovalRequests.Include(lr => lr.LeaveRequest).FirstOrDefault(x => x.ID == id);
                    if (request != null && currentUser.ID == request.ApproverID || request != null && currentUser.Role == 1)
                    {
                        var employee = _dbContext.Employees.FirstOrDefault(x => x.ID == request.LeaveRequest.EmployeeID);

                        ViewBag.daysOff = employee.Balance;
                        ViewBag.daysOffResult = employee.Balance - (request.LeaveRequest.EndDate.Day - request.LeaveRequest.StartDate.Day);

                        return View("EditForm", request);
                    }
                    return NotFound();
                default:
					return View("NoAccess");
			}
		}

		// POST: approvalreq/edit
		[HttpPost]
		[Route("edit")]
		public IActionResult Edit(ApprovalReq approvalRequest)
		{
			var existingRequest = _dbContext.ApprovalRequests
				.Include(lr => lr.LeaveRequest)
				.ThenInclude(e => e.Employee)
				.FirstOrDefault(x => x.ID == approvalRequest.ID);
			if (existingRequest != null)
			{
				if (approvalRequest.ApproverID != null)
				{
					var approver = _dbContext.Employees.FirstOrDefault(x => x.ID == approvalRequest.ApproverID);
					if (approver != null)
					{
						existingRequest.Approver = approver;
					}
				}
				existingRequest.ApproverID = approvalRequest.ApproverID;
				existingRequest.LeaveRequestID = approvalRequest.LeaveRequestID;
				existingRequest.Status = approvalRequest.Status;
				existingRequest.Comment = approvalRequest.Comment;


				if (existingRequest.Status == "Approved")
				{
					var otherApprovalRequests = _dbContext.ApprovalRequests
							.Include(lr => lr.LeaveRequest)
							.Where(ar => ar.LeaveRequestID == existingRequest.LeaveRequestID && ar.ID != existingRequest.ID)
							.ToList();

					_dbContext.ApprovalRequests.RemoveRange(otherApprovalRequests);

					existingRequest.LeaveRequest.Status = "Approved";
					existingRequest.LeaveRequest.Comment = existingRequest.Comment;
					existingRequest.LeaveRequest.Employee.Balance -= (existingRequest.LeaveRequest.EndDate.Day - existingRequest.LeaveRequest.StartDate.Day);

                }
                if (existingRequest.Status == "Declined")
                {
					existingRequest.LeaveRequest.Status = "Declined";
					existingRequest.LeaveRequest.Comment = existingRequest.Comment;
				}
				_dbContext.SaveChanges();

				return RedirectToAction("Index");
			}
			else
			{
				return NotFound();
			}
		}

		// GET: approvalreq/deleteform/id
		[HttpGet]
		[Route("deleteform/{id}")]
		public IActionResult DeleteForm(int id)
		{
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return View("NoAccess");
            }

			switch(currentUser.Role)
			{
				case 1:
				case 2:
				case 3:
                    var request = _dbContext.ApprovalRequests.FirstOrDefault(x => x.ID == id);
                    if (request != null)
                    {
                        if (currentUser.Role == request.ApproverID || currentUser.Role == 1)
                        {
                            return View("DeleteForm", request);
                        }            
                    }
                    return NotFound();
                default:
					return View("NoAccess");
			}
		}

		// POST: approvalreq/delete
		[HttpPost]
		[Route("delete")]
		public IActionResult Delete(int id)
		{
			var request = _dbContext.ApprovalRequests.FirstOrDefault(x => x.ID == id);
			if (request != null)
			{
				_dbContext.ApprovalRequests.Remove(request);

				_dbContext.SaveChanges();

				return RedirectToAction("Index");
			}
			return NotFound();
		}
	}
}
