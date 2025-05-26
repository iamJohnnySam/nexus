using Microsoft.EntityFrameworkCore;
using NexusModels;
using NexusModels.People;
using NexusModels.ProjectReview;
using NexusModels.ProjectTasks;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.DB;

public class AppDbContext : DbContext
{
    // People
    public DbSet<Designation> Designations { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Employee> Employees { get; set; }

    // Project
    public DbSet<Module> Modules { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Project> Projects { get; set; }

    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewPoint> ReviewPoints { get; set; }
    public DbSet<ProjectReviewItem> ProjectReviews { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=appdata.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>()
            .HasMany(t => t.SubTasks)
            .WithOne(t => t.ParentTaskItem)
            .HasForeignKey(t => t.ParentTaskItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasMany(t => t.Responsible)
            .WithMany(e => e.Tasks);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Modules)
            .WithMany(m => m.Projects)
            .UsingEntity(j => j.ToTable("ProjectModules"));

        modelBuilder.Entity<Project>()
            .HasMany(p => p.TaskItems)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Project)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProjectId);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.ReviewPoint)
            .WithMany()
            .HasForeignKey(r => r.ReviewPointId);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.ReviewItem)
            .WithOne()
            .HasForeignKey<Review>(r => r.ReviewId); // 1:1 between ProjectReview and ProjectReviewItem
    }
}
