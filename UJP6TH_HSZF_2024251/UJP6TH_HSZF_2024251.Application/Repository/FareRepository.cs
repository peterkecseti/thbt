using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UJP6TH_HSZF_2024251.Model;
using UJP6TH_HSZF_2024251.Persistence.MsSql;

namespace UJP6TH_HSZF_2024251.Application.Repository
{
    interface IFareRepository
    {
        Task Add(Fare fare, TaxiCar car);
    }
    public class FareRepository : IFareRepository
    {
        private readonly TaxiDbContext _dbContext;
        public FareRepository(TaxiDbContext context)
        {
            _dbContext = context;
        }
        public async Task Add(Fare fare, TaxiCar car)
        {
            car.Fares.Add(fare);
            await _dbContext.SaveChangesAsync();
        }
    }
}
