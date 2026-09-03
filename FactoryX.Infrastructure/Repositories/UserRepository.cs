using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
	public UserRepository(AppDbContext context) : base(context)
	{
	}
}
