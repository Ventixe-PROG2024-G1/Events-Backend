using EventsWebApi.Data.Context;
using EventsWebApi.Data.Entities;

namespace EventsWebApi.Repositories;

public interface ICategoryRepository : IBaseRepository<CategoryEntity>
{
}

public class CategoryRepository(ApplicationDbContext context) : BaseRepository<CategoryEntity>(context), ICategoryRepository
{
}
