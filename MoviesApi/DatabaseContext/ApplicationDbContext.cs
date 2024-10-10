using Microsoft.EntityFrameworkCore;
using MoviesApi.Models;
using System;

namespace MoviesApi.DatabaseContext
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor accepting DbContextOptions
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Film> Film { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}