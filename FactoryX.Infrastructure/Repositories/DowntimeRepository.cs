using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Infrastructure.Repositories;

public class DowntimeRepository : BaseRepository<Downtime>, IDowntimeRepository
{
	public DowntimeRepository(AppDbContext context) : base(context)
	{
	}
}
