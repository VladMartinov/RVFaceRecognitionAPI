using Microsoft.EntityFrameworkCore;

namespace RVFaceRecognitionAPI.Models
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserStatus> UserStatuses { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<TypeAction> TypeActions { get; set; }

        public DbSet<HistoryRecord> HistoryRecords { get; set; }
        public DbSet<Image> Images { get; set; }
    }
}
