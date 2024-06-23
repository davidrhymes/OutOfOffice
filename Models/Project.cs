using System.ComponentModel.DataAnnotations;

namespace OutOfOffice.Models
{
	public class Project
	{
		[Key]
		public int ID { get; set; }
		public string ProjectType { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set;}
		public int ProjectManagerID { get; set; }
		public string? Comment { get; set; }
		public string Status { get; set; }
		public virtual Employee ProjectManager { get; set; }
		public virtual ICollection<ProjectEmployee> ProjectEmployees { get; set; }

		public Project()
		{
			ProjectEmployees = new HashSet<ProjectEmployee>();
		}
	}
}
