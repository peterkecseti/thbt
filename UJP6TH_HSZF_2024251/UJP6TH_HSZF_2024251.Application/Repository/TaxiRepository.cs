using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UJP6TH_HSZF_2024251.Application.Dto;
using UJP6TH_HSZF_2024251.Model;
using UJP6TH_HSZF_2024251.Persistence.MsSql;

namespace UJP6TH_HSZF_2024251.Application.Repository
{
    namespace UJP6TH_HSZF_2024251.Application
    {
        public interface ITaxiRepository
        {
            Task<int> Add<T>(T entity) where T : class;
            Task<int> Remove<T>(T entity) where T : class;
            List<TaxiCar> GetAllCars();
            Task<int> UpdateCar(TaxiCar car);
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

            public List<TaxiCar> GetAllCars()
            {
                return context.TaxiCars.Include(c => c.Fares).ToListAsync().Result;
            }

            public async Task<int> Remove<T>(T entity) where T : class
            {
                context.Set<T>().Remove(entity);
                return await context.SaveChangesAsync();
            }

            public async Task<int> UpdateCar(TaxiCar car)
            {
                context.TaxiCars.Update(car);
                return await context.SaveChangesAsync();
            }
        }
    }
}

