using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UJP6TH_HSZF_2024251.Application.Repository.UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Application.Repository;
using UJP6TH_HSZF_2024251.Application.Services;
using UJP6TH_HSZF_2024251.Model.Entities;

namespace UJP6TH_HSZF_2024251.Test
{
    [TestFixture]
    public class Statistics_Tests
    {
        [Test]
        public async Task GenerateStatistics_ReturnsCorrectStatistics_MatchingJsonExample()
        {
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var mockedFareService = new Mock<IFareService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object, mockedFareService.Object);

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
    }
}
