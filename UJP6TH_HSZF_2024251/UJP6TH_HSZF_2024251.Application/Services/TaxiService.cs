using Newtonsoft.Json;
using Spectre.Console;
using UJP6TH_HSZF_2024251.Application.Dto;
using UJP6TH_HSZF_2024251.Application.Repository;
using UJP6TH_HSZF_2024251.Application.Repository.UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Model.Entities;


namespace UJP6TH_HSZF_2024251.Application.Services
{
    public interface ITaxiService
    {
        Task AddData(string path);
        Task<List<TaxiCar>> GetAllCars();
        //Task<List<TaxiCar>> PrintCarsTree(List<TaxiCar> cars);
        Task AddCar(string licensePlate, string driver);
        Task<bool> LicensePlateExists(string licensePlate);
        Task DeleteCar(TaxiCar toDelete);
        Task UpdateCar(TaxiCar toUpdate, string? newLicensePlate = null);
        Task AddFareToCar(string from, string to, int distance, int paidAmount, TaxiCar selectedCar);
        List<TaxiCar> FilterByLicensePlate(List<TaxiCar> cars, string filterValue);
        List<TaxiCar> FilterByDriver(List<TaxiCar> cars, string filterValue);
        List<TaxiCar> FilterByFromLocation(List<TaxiCar> cars, string filterValue);
        List<TaxiCar> FilterByToLocation(List<TaxiCar> cars, string filterValue);
        List<TaxiCar> FilterByDistance(List<TaxiCar> cars, int filterValue, Func<int, int, bool> comparisonFunc);
        List<TaxiCar> FilterByPaidAmount(List<TaxiCar> cars, int filterValue, Func<int, int, bool> comparisonFunc);
        Task<List<TaxiCar>> Filter(List<Func<List<TaxiCar>, List<TaxiCar>>> filterActions);
        void GenerateStatistics();
    }
    public class TaxiService : ITaxiService
    {
        public ITaxiRepository taxiContext;
        public IFareRepository fareContext;
        public TaxiService(ITaxiRepository taxiContext, IFareRepository fareContext)
        {
            this.taxiContext = taxiContext;
            this.fareContext = fareContext;
            
        }

        // event
        public delegate void FareWarningEventHandler(object sender);
        public event FareWarningEventHandler FareWarning;


        
        public async Task AddData(string path)
        {
            // Read JSON content from the provided path
            var json = File.ReadAllText(path); // Use the path parameter
            var taxiData = JsonConvert.DeserializeObject<RootTaxiDto>(json);

            // Iterate through the deserialized data and add it to the context
            foreach (var taxiCarData in taxiData.TaxiCars)
            {
                string licensePlate = taxiCarData.LicensePlate;
                var existingTaxiCar = await taxiContext.GetExistingCar(licensePlate);

                TaxiCar taxiCar;
                if (existingTaxiCar != null)
                {
                    taxiCar = existingTaxiCar;
                }
                else
                {
                    taxiCar = new TaxiCar(
                        taxiCarData.LicensePlate,
                        taxiCarData.Driver);
                    await taxiContext.Add(taxiCar);
                }

                foreach (var fareData in taxiCarData.Fares)
                {
                    var fare = new Fare(
                        fareData.From,
                        fareData.To,
                        fareData.Distance,
                        fareData.PaidAmount,
                        fareData.FareStartDate,
                        taxiCar);

                    await taxiContext.Add(fare);
                }
            }
        }

        public async Task<List<TaxiCar>> GetAllCars()
        {
            return await taxiContext.GetAllCars();
        }
        public async Task AddCar(string licensePlate, string driver)
        {
            // Save to database
            if (!LicensePlateExists(licensePlate).Result)
            {
                TaxiCar newCar = new TaxiCar(licensePlate, driver);
                await taxiContext.Add(newCar);
            }
            else
            {
                throw new LicensePlateException();
            }
        }
        public async Task<bool> LicensePlateExists(string licensePlate)
        {
            var taxiCar = await taxiContext.GetExistingCar(licensePlate);

            if (taxiCar == null) return false;
            else return taxiCar.LicensePlate == licensePlate;
        }
        public async Task DeleteCar(TaxiCar toDelete) => await taxiContext.Remove(toDelete);
        public async Task UpdateCar(TaxiCar toUpdate, string newLicensePlate = null)
        {
            if (!string.IsNullOrEmpty(newLicensePlate))
            {
                bool licenseExists = await LicensePlateExists(newLicensePlate);
                if (licenseExists)
                {
                    throw new LicensePlateException();
                }

                toUpdate.LicensePlate = newLicensePlate;
            }

            await taxiContext.UpdateCar(toUpdate);
        }
        public async Task AddFareToCar(string from, string to, int distance, int paidAmount, TaxiCar selectedCar)
        {
            DateTime now = DateTime.Now;
            Fare fare = new Fare(from, to, distance, paidAmount, now, selectedCar);

            var allFares = fareContext.GetAllFares().Result;
            Fare.CheckForHighPaidAmount(fare, allFares);

            await taxiContext.Add(fare);
        }

        public List<TaxiCar> FilterByLicensePlate(List<TaxiCar> cars, string filterValue)
        {
            var filteredCars = cars.Where(l => l.LicensePlate.ToLower().Contains(filterValue.ToLower())).ToList();
            return filteredCars;
        }

        public List<TaxiCar> FilterByDriver(List<TaxiCar> cars, string filterValue)
        {
            var filteredCars = cars.Where(l => l.Driver.ToLower().Contains(filterValue.ToLower())).ToList();
            return filteredCars;
        }

        public List<TaxiCar> FilterByFromLocation(List<TaxiCar> cars, string filterValue)
        {
            var filteredCars = cars
                    .Where(l => l.Fares.Any(f => f.From.ToLower().Contains(filterValue.ToLower())))
                    .Select(car => new TaxiCar(
                        car.Driver,
                        car.LicensePlate,
                        car.Fares.Where(f => f.From.ToLower().Contains(filterValue.ToLower())).ToList()))
                            .Where(car => car.Fares.Any()).ToList();
            return filteredCars;
        }

        public List<TaxiCar> FilterByToLocation(List<TaxiCar> cars, string filterValue)
        {
            var filteredCars = cars
                    .Where(l => l.Fares.Any(f => f.To.ToLower().Contains(filterValue.ToLower())))
                    .Select(car => new TaxiCar(
                        car.Driver,
                        car.LicensePlate,
                        car.Fares.Where(f => f.To.ToLower().Contains(filterValue.ToLower())).ToList()))
                            .Where(car => car.Fares.Any()).ToList();
            return filteredCars;
        }

        public List<TaxiCar> FilterByDistance(List<TaxiCar> cars, int filterValue, Func<int, int, bool> comparisonFunc)
        {
            var filteredCars = cars
                    .Where(car => car.Fares.Any(fare => comparisonFunc(fare.Distance, filterValue)))
                    .Select(car => new TaxiCar(
                        car.LicensePlate,
                        car.Driver,
                        car.Fares.Where(fare => comparisonFunc(fare.Distance, filterValue)).ToList()))
                            .Where(car => car.Fares.Any()).ToList();
            return filteredCars;
        }

        public List<TaxiCar> FilterByPaidAmount(List<TaxiCar> cars, int filterValue, Func<int, int, bool> comparisonFunc)
        {
            var filteredCars = cars
                    .Where(car => car.Fares.Any(fare => comparisonFunc(fare.PaidAmount, filterValue)))
                    .Select(car => new TaxiCar(
                        car.LicensePlate,
                        car.Driver,
                        car.Fares.Where(fare => comparisonFunc(fare.PaidAmount, filterValue)).ToList()))
                            .Where(car => car.Fares.Any()).ToList();
            return filteredCars;
        }
        public async Task<List<TaxiCar>> Filter(List<Func<List<TaxiCar>, List<TaxiCar>>> filterActions)
        {
            var cars = await taxiContext.GetAllCars();

            foreach (var filter in filterActions)
            {
                cars = filter(cars);
            }

            return cars;
        }
        public async void GenerateStatistics()
        {
            var cars = await taxiContext.GetAllCars();
            var statistics = cars
              .Select(car => new
              {
                  car.LicensePlate,
                  ShortTripsCount = car.Fares.Count(f => f.Distance < 10),
                  DistanceStatistics = new
                  {
                      AverageDistance = car.Fares.Any() ? car.Fares.Average(f => f.Distance) : 0,
                      ShortestTrip = car.Fares.OrderBy(f => f.Distance)
                        .Select(f => new { f.From, f.To, f.Distance, f.PaidAmount, f.FareStartDate })
                        .FirstOrDefault(),
                      LongestTrip = car.Fares.OrderByDescending(fare => fare.Distance)
                        .Select(f => new { f.From, f.To, f.Distance, f.PaidAmount, f.FareStartDate })
                        .FirstOrDefault()
                  },
                  MostFrequentDestination = car.Fares
                  .GroupBy(f => f.To)
                  .OrderByDescending(g => g.Count())
                  .Select(g => new
                  {
                      Destination = g.Key,
                      Count = g.Count()
                  })
                  .FirstOrDefault()
              })
              .ToList();

            var json = JsonConvert.SerializeObject(statistics, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            File.WriteAllText("A:\\progi\\szfa\\statistics.json", json);
            AnsiConsole.MarkupLine("[green]Statisztika generálva![/]");
            Console.ReadKey();


        }
    }
}
