using System.ComponentModel.DataAnnotations;

namespace OutOfOffice.Models
{
	public class ApprovalReq
	{
		[Key]
		public int ID { get; set; }
		public int ApproverID { get; set; }
		public int LeaveRequestID { get; set; }
		public string Status { get; set; }
		public string? Comment { get; set; }
		public virtual Employee Approver { get; set; }
		public virtual LeaveReq LeaveRequest { get; set; }
	}
}
