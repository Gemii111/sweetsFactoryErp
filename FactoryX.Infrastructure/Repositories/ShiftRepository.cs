using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Infrastructure.Repositories;

public class ShiftRepository : BaseRepository<Shift>, IShiftRepository
{
	public ShiftRepository(AppDbContext context) : base(context)
	{
	}
}
