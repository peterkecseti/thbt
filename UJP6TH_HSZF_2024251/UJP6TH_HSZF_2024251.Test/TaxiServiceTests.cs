using Moq;
using NUnit.Framework;
using System.Net.WebSockets;
using UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Application.Repository;
using UJP6TH_HSZF_2024251.Application.Repository.UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Application.Services;
using UJP6TH_HSZF_2024251.Model.Entities;

namespace UJP6TH_HSZF_2024251.Test
{
    [TestFixture]
    public class TaxiServiceTests
    {
        [Test]
        public async Task AddData_DoesNotAddNewCarIfAlreadyExists()
        {
            var jsonData = @"
            {
            ""TaxiCars"": [
                    {
                    ""LicensePlate"": ""DA AA-333"",
                    ""Driver"": ""Bartos Csaba István"",
                    ""Fares"": [
                            {
                            ""From"": ""Budapest, Népliget"",
                            ""To"": ""Dabas, buszállomás"",
                            ""Distance"": 50,
                            ""PaidAmount"": 22000,
                            ""FareStartDate"": ""2024-08-15T00:00:00Z""
                            }
                        ]
                    }
                ]
            }";

            File.WriteAllText("test.json", jsonData);

            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();

            var existingTaxiCar = new TaxiCar("DA AA-333", "Bartos Csaba István");
            mockedTaxiRepository
                .Setup(ctx => ctx.GetExistingCar("DA AA-333"))
                .ReturnsAsync(existingTaxiCar);

            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object);

            await taxiService.AddData("test.json");

            mockedTaxiRepository.Verify(ctx => ctx.Add(It.IsAny<TaxiCar>()), Times.Never);
            mockedTaxiRepository.Verify(ctx => ctx.Add(It.Is<Fare>(f =>
                f.From == "Budapest, Népliget" &&
                f.To == "Dabas, buszállomás" &&
                f.Distance == 50 &&
                f.PaidAmount == 22000)),
                Times.Once);
        }

        [Test]
        public async Task AddCar_WhenLicensePlateDoesNotExistAddsNewCar()
        {

            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object);

            string licensePlate = "DA AA-333";
            string driver = "Bartos Csaba István";

            mockedTaxiRepository
                .Setup(ctx => ctx.GetExistingCar(licensePlate))
                .ReturnsAsync((TaxiCar)null);

            await taxiService.AddCar(licensePlate, driver);

            mockedTaxiRepository.Verify(ctx => ctx.Add(It.Is<TaxiCar>(car =>
                car.LicensePlate == licensePlate &&
                car.Driver == driver)),
                Times.Once);
        }
        [Test]
        public async Task AddCar_WhenLicensePlateExistsThrowsLicensePlateException()
        {

            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object);


            string licensePlate = "DA AA-333";
            string driver = "Bartos Csaba István";

            var existingTaxiCar = new TaxiCar(licensePlate, driver);
            mockedTaxiRepository
                .Setup(ctx => ctx.GetExistingCar(licensePlate))
                .ReturnsAsync(existingTaxiCar);

            Assert.ThrowsAsync<LicensePlateException>(() => taxiService.AddCar(licensePlate, driver));

            mockedTaxiRepository.Verify(ctx => ctx.Add(It.IsAny<TaxiCar>()), Times.Never);
        }
        [Test]
        public async Task GenerateStatistics_ReturnsCorrectStatistics_MatchingJsonExample()
        {
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object);

            var cars = new List<TaxiCar>{
                new TaxiCar("DA AA-333", "Bartos Csaba István", new List<Fare>
                    {
                        new Fare("Budapest, Keleti Pályaudvar", "Gyál, Vasútállomás", 20, 15000, DateTime.Parse("2024-09-05T09:30:00Z"), null),
                        new Fare("Budapest, Népliget", "Dabas, buszállomás", 50, 22000, DateTime.Parse("2024-08-15T00:00:00Z"), null)
                    })
                };

            foreach (var car in cars)
            {
                foreach (var fare in car.Fares)
                {
                    fare.TaxiCar = car;
                }
            }

            mockedTaxiRepository.Setup(ctx => ctx.GetAllCars()).ReturnsAsync(cars);

            var statistics = await taxiService.GenerateStatistics();
            Assert.That(statistics.Count, Is.EqualTo(1));

            var car1Stats = statistics[0];
            Assert.That(car1Stats.ShortTripsCount, Is.EqualTo(0));
            Assert.That(car1Stats.DistanceStatistics.AverageDistance, Is.EqualTo(35));
            Assert.That(car1Stats.DistanceStatistics.ShortestTrip.From, Is.EqualTo("Budapest, Keleti Pályaudvar"));
            Assert.That(car1Stats.DistanceStatistics.ShortestTrip.To, Is.EqualTo("Gyál, Vasútállomás"));
            Assert.That(car1Stats.DistanceStatistics.ShortestTrip.Distance, Is.EqualTo(20));
            Assert.That(car1Stats.DistanceStatistics.ShortestTrip.PaidAmount, Is.EqualTo(15000));
            Assert.That(car1Stats.DistanceStatistics.LongestTrip.From, Is.EqualTo("Budapest, Népliget"));
            Assert.That(car1Stats.DistanceStatistics.LongestTrip.To, Is.EqualTo("Dabas, buszállomás"));
            Assert.That(car1Stats.DistanceStatistics.LongestTrip.Distance, Is.EqualTo(50));
            Assert.That(car1Stats.DistanceStatistics.LongestTrip.PaidAmount, Is.EqualTo(22000));
            Assert.That(car1Stats.MostFrequentDestination.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateCar_ChangesLicensePlateIfUnique()
        {
            // Arrange
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var mockedTaxiService = new Mock<ITaxiService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object);
            var taxiCar = new TaxiCar("OLD-PLATE", "gasparlaci");
            string newLicensePlate = "NEW-PLATE";

            mockedTaxiService.Setup(ctx => ctx.LicensePlateExists(newLicensePlate)).ReturnsAsync(false);

            await taxiService.UpdateCar(taxiCar, newLicensePlate);

            Assert.That(taxiCar.LicensePlate, Is.EqualTo(newLicensePlate));
            mockedTaxiRepository.Verify(ctx => ctx.UpdateCar(taxiCar), Times.Once);
        }

        [Test]
        public async Task UpdateCar_ExceptionOnDuplicateLicensePlate()
        {
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object);

            var taxiCar = new TaxiCar("OLD-PLATE", "gasparlaci");
            string duplicateLicensePlate = "NEW-PLATE";

            mockedTaxiRepository
                .Setup(repo => repo.GetExistingCar(duplicateLicensePlate))
                .ReturnsAsync(new TaxiCar(duplicateLicensePlate, "gasparlaci"));

            Assert.ThrowsAsync<LicensePlateException>(() => taxiService.UpdateCar(taxiCar, duplicateLicensePlate));

            mockedTaxiRepository.Verify(ctx => ctx.UpdateCar(It.IsAny<TaxiCar>()), Times.Never);
        }

        [Test]
        public async Task UpdateCar_NoLicensePlateUpdateIfLicensePlateIsEmpty()
        {
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var mockedTaxiService = new Mock<ITaxiService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object);
            var taxiCar = new TaxiCar("OLD-PLATE", "gasparlaci");

            await taxiService.UpdateCar(taxiCar);

            Assert.That(taxiCar.LicensePlate, Is.EqualTo("OLD-PLATE"));
            mockedTaxiRepository.Verify(ctx => ctx.UpdateCar(taxiCar), Times.Once);
        }

        [Test]
        public async Task UpdateCar_CallsUpdateWhenCalledFromService()
        {
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var mockedTaxiService = new Mock<ITaxiService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object);
            var taxiCar = new TaxiCar("OLD-PLATE", "gasparlaci");

            await taxiService.UpdateCar(taxiCar);

            mockedTaxiRepository.Verify(ctx => ctx.UpdateCar(taxiCar), Times.Once);
        }
        [Test]
        public async Task Filter_Test()
        {
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object);

            var expected = new TaxiCar("DA AA-333", "Bartos Csaba István");

            var expectedFare = new Fare(
                "Budapest, Keleti Pályaudvar",
                "Gyál, Vasútállomás",
                20,
                15000,
                DateTime.Parse("2024-09-05T09:30:00Z"),
                expected);

            expected.Fares.Add(expectedFare);

            var cars = new List<TaxiCar> { expected };
            mockedTaxiRepository.Setup(repo => repo.GetAllCars()).ReturnsAsync(cars);

            List<Func<List<TaxiCar>, List<TaxiCar>>> filterActions = new List<Func<List<TaxiCar>, List<TaxiCar>>>()
                {
                    cars => taxiService.FilterByLicensePlate(cars, "DA AA"),
                    cars => taxiService.FilterByDriver(cars, "Bartos"),
                    cars => taxiService.FilterByPaidAmount(cars, 15000, (actual, filterValue) => actual == filterValue),
                    cars => taxiService.FilterByDistance(cars, 15, (actual, filterValue) => actual > filterValue)
                };

            var result = await taxiService.Filter(filterActions);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(expected));
        }

        [Test]
        public async Task AddFareToCar_EventTriggerTest()
        {
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object);

            var car = new TaxiCar("", "");

            var existingFare = new Fare("", "", 1, 1, DateTime.Now.AddDays(1), car);
            var newFare = new Fare("", "", 1, 9999999, DateTime.Now, car);

            mockedFareRepository.Setup(repo => repo.GetAllFares()).ReturnsAsync(new List<Fare> { existingFare });

            mockedTaxiRepository.Setup(repo => repo.Add(It.IsAny<Fare>())).ReturnsAsync(1);

            Fare detectedFare = null;
            Fare.HighPaidAmountDetected += fare => detectedFare = fare;

            await taxiService.AddFareToCar(newFare.From, newFare.To, newFare.Distance, newFare.PaidAmount, car);

            Assert.That(detectedFare, Is.Not.Null);
            Assert.That(detectedFare.PaidAmount, Is.EqualTo(newFare.PaidAmount));
            Assert.That(detectedFare.TaxiCar, Is.EqualTo(car));

            mockedTaxiRepository.Verify(repo => repo.Add(It.Is<Fare>(f =>
                f.From == newFare.From &&
                f.To == newFare.To &&
                f.Distance == newFare.Distance &&
                f.PaidAmount == newFare.PaidAmount &&
                f.TaxiCar == car
            )), Times.Once);
        }
    }
}
