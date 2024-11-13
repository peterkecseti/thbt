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
        Task<List<TaxiCar>> Filter(List<Func<List<TaxiCar>, List<TaxiCar>>> filterActions, List<TaxiCar> toFilter);
        Task<List<TaxiCarStatistics>> GenerateStatistics();
    }
    public class TaxiService : ITaxiService
    {
        public ITaxiRepository taxiContext;
        public IFareRepository fareContext;
        public IFareService fareService;
        public TaxiService(ITaxiRepository taxiContext, IFareRepository fareContext, IFareService fareService)
        {
            this.taxiContext = taxiContext;
            this.fareContext = fareContext;
            this.fareService = fareService;
            fareService.HighPaidAmountDetected += OnHighPaidAmountDetected;
        }

        // event
        public event Action<Fare> HighPaidAmountDetected;
        private void OnHighPaidAmountDetected(Fare highPaidFare) => HighPaidAmountDetected?.Invoke(highPaidFare);


        public async Task AddData(string path)
        {
            var json = File.ReadAllText(path);
            var taxiData = JsonConvert.DeserializeObject<RootTaxiDto>(json);
            if (taxiData == null || taxiData.TaxiCars == null) throw new BadJsonException(); // ♪

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
        public async Task DeleteCar(TaxiCar toDelete) => await taxiContext.Remove(toDelete);
        public async Task AddFareToCar(string from, string to, int distance, int paidAmount, TaxiCar selectedCar)
        {
            DateTime now = DateTime.Now;
            Fare fare = new Fare(from, to, distance, paidAmount, now, selectedCar);

            var allFares = await fareService.GetAllFares();

            // check if event should be triggered
            fareService.CheckForHighPaidAmount(fare, allFares);

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
        public async Task<List<TaxiCar>> Filter(List<Func<List<TaxiCar>, List<TaxiCar>>> filterActions, List<TaxiCar> toFilter = null) // ♪
        {
            var cars = new List<TaxiCar>();
            if (toFilter == null)
                cars = await taxiContext.GetAllCars();

            else cars = toFilter;

            // Run the entire cars list on the selected filtering options
            foreach (var filter in filterActions)
            {
                cars = filter(cars);
            }

            return cars;
        }
        public async Task<List<TaxiCarStatistics>> GenerateStatistics()
        {
            var cars = await taxiContext.GetAllCars();
            #pragma warning disable CS8604 // remove the annoying green underlines

            var statistics = cars.Select(car => new TaxiCarStatistics(
                car.LicensePlate,
                car.Fares.Count(f => f.Distance < 10),

                new DistanceStats(
                    car.Fares.Any() ? car.Fares.Average(f => f.Distance) : 0,
                    car.Fares.OrderBy(f => f.Distance)
                        .Select(f => new TripDetails(f.From, f.To, f.Distance, f.PaidAmount, f.FareStartDate))
                        .FirstOrDefault(),

                    car.Fares.OrderByDescending(f => f.Distance)
                        .Select(f => new TripDetails(f.From, f.To, f.Distance, f.PaidAmount, f.FareStartDate))
                        .FirstOrDefault()
                ),
                car.Fares.GroupBy(f => f.To)
                    .OrderByDescending(g => g.Count())
                    .Select(g => new FrequentDestination(g.Key, g.Count()))
                    .FirstOrDefault()
            )).ToList();

            #pragma warning restore CS8604 // restore the annoying green underlines

            return statistics;
        }
    }
}
