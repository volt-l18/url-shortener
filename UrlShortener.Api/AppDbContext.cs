using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortenedUrl> Urls => Set<ShortenedUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Video requirement: Speed up reads by creating an index on the short code column
        modelBuilder.Entity<ShortenedUrl>()
            .HasIndex(u => u.ShortCode)
            .IsUnique();
    }
}

