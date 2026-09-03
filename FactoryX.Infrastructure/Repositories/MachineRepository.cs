using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Infrastructure.Repositories;

public class MachineRepository : BaseRepository<Machine>, IMachineRepository
{
	public MachineRepository(AppDbContext context) : base(context)
	{

	}
}
