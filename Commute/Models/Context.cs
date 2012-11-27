using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Commute.Models
{
    public class Context : DbContext
    {
        public DbSet<Location> Locations { get; set; }
        public DbSet<Route> Route { get; set; }
        public DbSet<RouteWayPoint> RouteWayPoint { get; set; }
        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            //Remove plural for table names
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, Configuration>());

            //Set the precision for decimal - does not work on AppHarbor

            //User
            modelBuilder.Entity<User>().Property(model => model.LocationLatitude).HasPrecision(18, 14);
            modelBuilder.Entity<User>().Property(model => model.LocationLongitude).HasPrecision(18, 14);

            //Location
            modelBuilder.Entity<Location>().Property(location => location.Name).HasMaxLength(100);
            modelBuilder.Entity<Location>().Property(location => location.Latitude).HasPrecision(18, 14);
            modelBuilder.Entity<Location>().Property(location => location.Longitude).HasPrecision(18, 14);

            //Route
            modelBuilder.Entity<Route>().Property(model => model.StartLatitude).HasPrecision(18, 14);
            modelBuilder.Entity<Route>().Property(model => model.StartLongitude).HasPrecision(18, 14);
            modelBuilder.Entity<Route>().Property(model => model.EndLatitude).HasPrecision(18, 14);
            modelBuilder.Entity<Route>().Property(model => model.EndLongitude).HasPrecision(18, 14);

            //RouteWayPoint
            modelBuilder.Entity<RouteWayPoint>().Property(model => model.Latitude).HasPrecision(18, 14);
            modelBuilder.Entity<RouteWayPoint>().Property(model => model.Longitude).HasPrecision(18, 14);

        }
    }
}