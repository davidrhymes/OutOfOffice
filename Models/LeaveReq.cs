using System.ComponentModel.DataAnnotations;

namespace OutOfOffice.Models
{
	public class LeaveReq
	{
		[Key]
		public int ID { get; set; }
		public int EmployeeID { get; set; }
		public string AbsenceReason { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string? Comment { get; set; }
		public string Status { get; set; }
		public virtual Employee Employee { get; set; }
		public virtual ICollection<ApprovalReq> ApprovalRequests { get; set; }

		public LeaveReq()
		{
			ApprovalRequests = new HashSet<ApprovalReq>();
		}
	}
}
