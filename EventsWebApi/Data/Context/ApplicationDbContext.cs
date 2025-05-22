using EventsWebApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsWebApi.Data.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CategoryEntity>().HasData(
            new CategoryEntity { Id = Guid.Parse("58253062-17d0-48cd-8cb5-35ee499fb56d"), CategoryName = "Music" },
            new CategoryEntity { Id = Guid.Parse("7e95dcfb-2051-4983-bc68-e9adfeaa86d1"), CategoryName = "Health & Wellness" },
            new CategoryEntity { Id = Guid.Parse("a0b781b4-48fe-4331-b809-4ee25cb25301"), CategoryName = "Art & Design" },
            new CategoryEntity { Id = Guid.Parse("e910e72c-3d6f-4f30-b271-9e7cf46862a0"), CategoryName = "Technology" },
            new CategoryEntity { Id = Guid.Parse("7b536871-cc9f-491f-854d-289fefb93cc7"), CategoryName = "Food & Culinary" },
            new CategoryEntity { Id = Guid.Parse("8172ad1b-5298-48fd-a346-a4ed873484f3"), CategoryName = "Fashion" },
            new CategoryEntity { Id = Guid.Parse("987444d6-4256-4cc9-b76e-cb6c9be1875c"), CategoryName = "Outdoor & Activities" },
            new CategoryEntity { Id = Guid.Parse("db013c07-6c5d-4b7e-8d82-d2eab1a860f4"), CategoryName = "Film & Cinema" },
            new CategoryEntity { Id = Guid.Parse("13267c45-1219-4cea-ba4b-336b585dca96"), CategoryName = "Theater & Performing Arts" },
            new CategoryEntity { Id = Guid.Parse("915f321e-1ca7-4ebf-98c8-01e7f8188f68"), CategoryName = "Literature & Book Fairs" },
            new CategoryEntity { Id = Guid.Parse("ad2ccfe4-54d3-44c7-a0a5-d25e692376f1"), CategoryName = "Sports & Fitness" },
            new CategoryEntity { Id = Guid.Parse("207a467e-b44b-4c95-a0e9-f7e0ef66a455"), CategoryName = "Gaming & eSports" },
            new CategoryEntity { Id = Guid.Parse("c585c7a6-c5ac-4f52-83c2-fbcf0274e257"), CategoryName = "History & Heritage" },
            new CategoryEntity { Id = Guid.Parse("043c6f2a-adef-494b-9038-f8d7cdc154a8"), CategoryName = "Crafts & Hobbies" },
            new CategoryEntity { Id = Guid.Parse("16b2d573-ad6f-460d-8db4-168a6fce6044"), CategoryName = "Festival" },
            new CategoryEntity { Id = Guid.Parse("14fa8c26-d45f-48a4-9fab-214558ef179a"), CategoryName = "Community & Local Events" }
        );
    }
}
