using Microsoft.EntityFrameworkCore;
using OutOfOffice.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfOffice.Data
{
	public class AppDBContext : DbContext
	{
		public DbSet<Employee>? Employees { get; set; }
		public DbSet<LeaveReq>? LeaveRequests { get; set; }
		public DbSet<ApprovalReq>? ApprovalRequests { get; set; }
		public DbSet<Project>? Projects { get; set; }
		public DbSet<ProjectEmployee>? ProjectEmployees { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Employee>()
				.HasMany(e => e.LeaveRequests)
				.WithOne(l => l.Employee)
				.HasForeignKey(l => l.EmployeeID);

			modelBuilder.Entity<Employee>()
				.HasMany(e => e.ApprovalReq)
				.WithOne(a => a.Approver)
				.HasForeignKey(a => a.ApproverID);

			modelBuilder.Entity<Employee>()
				.HasMany(e => e.ManagedProjects)
				.WithOne(p => p.ProjectManager)
				.HasForeignKey(p => p.ProjectManagerID);

			modelBuilder.Entity<LeaveReq>()
				.HasMany(l => l.ApprovalRequests)
				.WithOne(a => a.LeaveRequest)
				.HasForeignKey(a => a.LeaveRequestID);

			modelBuilder.Entity<ProjectEmployee>()
			.HasKey(pe => new { pe.ProjectID, pe.EmployeeID });

			modelBuilder.Entity<ProjectEmployee>()
				.HasOne(pe => pe.Project)
				.WithMany(p => p.ProjectEmployees)
				.HasForeignKey(pe => pe.ProjectID);

			modelBuilder.Entity<ProjectEmployee>()
				.HasOne(pe => pe.Employee)
				.WithMany(e => e.ProjectEmployees)
				.HasForeignKey(pe => pe.EmployeeID);

			base.OnModelCreating(modelBuilder);
		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=DESKTOP-V00OB57;Initial Catalog=outofoffice;Trusted_Connection=True;TrustServerCertificate=True");
            }
        }
    }
}
