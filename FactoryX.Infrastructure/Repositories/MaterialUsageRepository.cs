using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Infrastructure.Repositories;

public class MaterialUsageRepository : BaseRepository<MaterialUsage>, IMaterialUsageRepository
{
    public MaterialUsageRepository(AppDbContext context) : base(context)
    {
    }
}