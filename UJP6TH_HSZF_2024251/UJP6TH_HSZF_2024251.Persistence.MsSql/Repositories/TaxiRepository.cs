using Microsoft.EntityFrameworkCore;
using UJP6TH_HSZF_2024251.Model.Entities;
using UJP6TH_HSZF_2024251.Persistence.MsSql;

namespace UJP6TH_HSZF_2024251.Application.Repository
{
    namespace UJP6TH_HSZF_2024251.Application
    {
        public interface ITaxiRepository
        {
            Task<int> Add<T>(T entity) where T : class;
            Task<int> Remove<T>(T entity) where T : class;
            Task<List<TaxiCar>> GetAllCars();
            Task UpdateCar(TaxiCar car);
            Task<TaxiCar> GetExistingCar(string licensePlate);
        }
        public class TaxiRepository : ITaxiRepository
        {
            private readonly TaxiDbContext context;
            public TaxiRepository(TaxiDbContext context)
            {
                this.context = context;
            }
            public async Task<int> Add<T>(T entity) where T : class
            {
                context.Set<T>().Add(entity);
                return await context.SaveChangesAsync();
            }

            public async Task<List<TaxiCar>> GetAllCars()
            {
                return await context.TaxiCars.Include(c => c.Fares).ToListAsync();
            }

            public async Task<int> Remove<T>(T entity) where T : class
            {
                context.Set<T>().Remove(entity);
                return await context.SaveChangesAsync();
            }

            public async Task UpdateCar(TaxiCar car)
            {
                context.TaxiCars.Update(car);
                await context.SaveChangesAsync();
            }
            public async Task<TaxiCar> GetExistingCar(string licensePlate)
            {
                var car = await context.TaxiCars.Include(c => c.Fares).FirstOrDefaultAsync(tc => tc.LicensePlate == licensePlate);
                return car;
            }
        }
    }
}

