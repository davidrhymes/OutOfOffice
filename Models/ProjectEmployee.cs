namespace OutOfOffice.Models
{
	public class ProjectEmployee
	{
		public int ProjectID { get; set; }
		public Project Project { get; set; }

		public int EmployeeID { get; set; }
		public Employee Employee { get; set; }
	}
}
