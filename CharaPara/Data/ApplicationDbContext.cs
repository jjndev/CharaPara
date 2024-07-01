using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using CharaPara.Data.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CharaPara.Data
{

    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //use MySQL; get connectionstring from appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            //optionsBuilder.UseMySQL(configuration.GetConnectionString("DefaultConnection"));


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
               .HasMany(u => u.Record_AppUserInfractions)
               .WithOne(i => i.AppUser)
               .HasForeignKey(i => i.AppUserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Record_ImageUpload>()
                .HasOne<AppUser>(riu => riu.AppUser) 
                .WithMany() 
                .HasForeignKey(riu => riu.AppUserId); 
        }

        public DbSet<SearchTag> SearchTags { get; set; }

        public DbSet<Record_ImageUpload> Record_ImageUploads { get; set; }

        public DbSet<Tab> Tabs { get; set; }

        public DbSet<GalleryImage> GalleryImages { get; set; }

        public DbSet<Profile> Profiles { get; set; }



        //public DbSet<TabTemplate> TabTemplate { get; set; }
    }
}
