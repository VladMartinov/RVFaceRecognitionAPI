﻿using Microsoft.EntityFrameworkCore;

namespace RVFaceRecognitionAPI.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext(DbContextOptions<UsersContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<UserStatus>().HasKey(us => us.StatusId);
            modelBuilder.Entity<UserRole>().HasKey(ur => ur.RolesId);

            modelBuilder.Entity<User>().HasOne(u => u.Role).WithMany(ur => ur.Users).HasForeignKey(u => u.RoleId);
            modelBuilder.Entity<User>().HasOne(u => u.Status).WithMany(us => us.Users).HasForeignKey(u => u.StatusId);
            */

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        /*
        public DbSet<UserStatus> UserStatuses { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        */
    }
}
