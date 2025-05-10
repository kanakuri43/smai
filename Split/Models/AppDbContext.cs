using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Split.Models;

namespace sale.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<WeeklyProgress> WeeklySalesProgresses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = LoadConfig();
            string connectionString = (config.ConnectionString).ToString();
            optionsBuilder.UseSqlServer(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeeklyProgress>()
                  .ToTable("S進捗")
                  .HasKey(wp => new { wp.Date, wp.EmployeeCode, wp.ProgressType });

        }

        static dynamic LoadConfig()
        {
            var doc = XDocument.Load("config.xml");

            return new
            {
                ConnectionString = doc.Root.Element("Database").Element("ConnectionString").Value,
            };
        }
    }
}
