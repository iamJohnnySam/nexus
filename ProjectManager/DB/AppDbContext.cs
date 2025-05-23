﻿using Microsoft.EntityFrameworkCore;
using NexusModels;
using ProjectManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<ReviewPoint> Reviews { get; set; }
        public DbSet<ProjectReviewItem> ProjectReviews { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=appdata.db");
        }
    }
}
 