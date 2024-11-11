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
            Task<List<TaxiCar>> GetAllCars();
            Task UpdateCar(TaxiCar car);
            Task<TaxiCar> GetExistingCar(string licensePlate);
            Task<TaxiCar> GetCarById(Guid id);
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

            public async Task UpdateCarDriver(Guid id, string driver)
            {
                var toUpdate = await context.TaxiCars.Include(c => c.Fares).SingleOrDefaultAsync(c => c.TaxiID == id);
                if (toUpdate != null)
                {
                    toUpdate.Driver = driver;
                    context.TaxiCars.Update(toUpdate);
                    await context.SaveChangesAsync();
                };
            }

            public async Task UpdateCarLicensePlate(Guid id, string LicensePlate)
            {
                var toUpdate = await context.TaxiCars.Include(c => c.Fares).SingleOrDefaultAsync(c => c.TaxiID == id);
                if (toUpdate != null)
                {
                    toUpdate.LicensePlate = LicensePlate;
                    context.TaxiCars.Update(toUpdate);
                    await context.SaveChangesAsync();
                };
            }

            public async Task<TaxiCar> GetCarById(Guid id)
            {
                var car = await context.TaxiCars.Include(c => c.Fares).SingleOrDefaultAsync(c => c.TaxiID == id);
                if (car != null) return car;
                else throw new Exception($"[red]No car found with id [/][red]{id}[/]");
            }

            public async Task<TaxiCar> GetExistingCar(string licensePlate)
            {
                var car = await context.TaxiCars.Include(c => c.Fares).FirstOrDefaultAsync(tc => tc.LicensePlate == licensePlate);

                //if (car == null) throw new LicensePlateException($"[red]No car found with license plate: {licensePlate}[/]");

                return car;
            }
        }
    }
}

