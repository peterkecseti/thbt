using Microsoft.EntityFrameworkCore;
using UJP6TH_HSZF_2024251.Model.Entities;
using UJP6TH_HSZF_2024251.Persistence.MsSql;

namespace UJP6TH_HSZF_2024251.Application.Repository
{
    public interface IFareRepository
    {
        Task<List<Fare>> GetAllFares();
    }
    public class FareRepository : IFareRepository
    {
        private readonly TaxiDbContext context;
        public FareRepository(TaxiDbContext context)
        {
            this.context = context;
        }
        public async Task<List<Fare>> GetAllFares()
        {
            return await context.Fares.ToListAsync();
        }
    }
}
