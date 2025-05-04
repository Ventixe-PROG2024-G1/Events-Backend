using EventsWebApi.Data.Context;
using EventsWebApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EventsWebApi.Repositories;

public interface IEventRepository : IBaseRepository<EventEntity>
{
    Task<IEnumerable<EventEntity>> GetEventsByCategoryIdAsync(Guid categoryId);
}

public class EventRepository(ApplicationDbContext context) : BaseRepository<EventEntity>(context), IEventRepository
{
    private IQueryable<EventEntity> Includes()
    {
        return _table
            .Include(x => x.Category);
    }

    public override async Task<IEnumerable<EventEntity>> GetAllAsync()
    {
        return await Includes().ToListAsync();
    }

    public override async Task<EventEntity?> GetByIdAsync(Expression<Func<EventEntity, bool>> predicate)
    {
        return await Includes().FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<EventEntity>> GetEventsByCategoryIdAsync(Guid categoryId)
    {
        return await Includes()
            .Where(x => x.Category.Id == categoryId)
            .ToListAsync();
    }
}
// Behövs override här? Vart mer behöver man Kategorier?