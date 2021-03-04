using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Order_Application.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class PassionDbContext : IdentityDbContext<ApplicationUser>
    {
        public PassionDbContext()
            : base("name=PassionDbContextwAuth", throwIfV1Schema: false)
        {
            base.Configuration.ProxyCreationEnabled = false;
        }

        public static PassionDbContext Create()
        {
            return new PassionDbContext();
        }

        //Instruction to set the models as tables in our database.
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }


        //Reference to explicitly describing a bridging table: https://social.technet.microsoft.com/wiki/contents/articles/28670.entity-framework-customized-join-table-in-a-many-to-many-relationship.aspx
        //Reference for OnModelCreating base:https://stackoverflow.com/questions/30315968/resolving-no-key-defined-errors-while-using-onmodelcreating-with-applicationdb
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<OrderItems>()
                .HasKey(oi => new { oi.OrderID, oi.ProductID });

            modelBuilder.Entity<OrderItems>()
                .HasRequired(oi => oi.Order)
                .WithMany(oi => oi.OrderItems)
                .HasForeignKey(oi => oi.OrderID);

            modelBuilder.Entity<OrderItems>()
                .HasRequired(oi => oi.Product)
                .WithMany(oi => oi.OrderItems)
                .HasForeignKey(oi => oi.ProductID);

        }

        public System.Data.Entity.DbSet<Order_Application.Models.OrderItems> OrderItems { get; set; }

        //To Run the database, use code-first migrations
        //https://www.entityframeworktutorial.net/code-first/code-based-migration-in-code-first.aspx

        //Tools > NuGet Package Manager > Package Manager Console
        //enable-migrations (only once)
        //add-migration {migration name}
        //update-database

        //To View the Database Changes sequentially, go to Project/Migrations folder

        //To View Database itself, go to View > SQL Server Object Explorer
        // (localdb)\MSSQLLocalDB
        // Right Click {ProjectName}.Models.DataContext > Tables
        // Can Right Click Tables to View Data
        // Can Right Click Database to Query

        // You will have to add in some example data to the database locally

    }
}