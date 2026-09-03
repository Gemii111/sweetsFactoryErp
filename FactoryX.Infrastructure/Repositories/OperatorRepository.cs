using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Infrastructure.Repositories;

public class OperatorRepository : BaseRepository<Operator>, IOperatorRepository
{
	public OperatorRepository(AppDbContext context) : base(context)
	{

	}
}
