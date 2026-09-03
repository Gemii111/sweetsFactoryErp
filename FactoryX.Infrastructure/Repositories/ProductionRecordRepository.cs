using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Infrastructure.Repositories;

public class ProductionRecordRepository : BaseRepository<ProductionRecord>, IProductionRecordRepository
{
    public ProductionRecordRepository(AppDbContext context) : base(context)
    {
    }
}

