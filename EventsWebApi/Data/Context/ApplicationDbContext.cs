using EventsWebApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsWebApi.Data.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
}
