using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfOffice.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }
        [Column("FullName")]
        public string FullName { get; set; }
        [Column("Subdivision")]
        public string Subdivision { get; set; }
        [Column("Position")]
        public string Position { get; set; }
        [Column("Status")]
        public string Status { get; set; }
        [Column("PeoplePartnerID")]
        public int? PeoplePartnerID { get; set; }
        [Column("Balance")]
        public decimal Balance { get; set; }
        [Column("Photo")]
        public byte[]? Photo { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
		public virtual Employee PeoplePartner { get; set; }
		public virtual ICollection<LeaveReq> LeaveRequests { get; set; }
		public virtual ICollection<ApprovalReq> ApprovalReq { get; set; }
		public virtual ICollection<Project> ManagedProjects { get; set; }
        public virtual ICollection<ProjectEmployee> ProjectEmployees { get; set; }

        public Employee()
        {
            LeaveRequests = new HashSet<LeaveReq>();
            ManagedProjects = new HashSet<Project>();
            ApprovalReq = new HashSet<ApprovalReq>();
			ProjectEmployees = new HashSet<ProjectEmployee>();
        }
	}
}
