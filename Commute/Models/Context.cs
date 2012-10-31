using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Commute.Models
{
    public class Context : DbContext
    {
        public DbSet<Location> Locations { get; set; }
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, Configuration>());

            //Set the precision for decimal - cannot compile
            modelBuilder.Entity<Location>().Property(location => location.Name).HasMaxLength(100); //Annotation does not work on AppHarbor
            modelBuilder.Entity<Location>().Property(location => location.Latitude).HasPrecision(18, 14);
            modelBuilder.Entity<Location>().Property(location => location.Longitude).HasPrecision(18, 14);
            //modelBuilder.Entity<Location>().Property(object => object.Latitude).HasPrecision(12, 10);
            //http://stackoverflow.com/questions/3504660/entity-framework-code-first-decimal-precision
        }
    }
}