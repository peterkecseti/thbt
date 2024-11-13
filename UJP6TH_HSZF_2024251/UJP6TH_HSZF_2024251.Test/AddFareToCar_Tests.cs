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
    internal class AddFareToCar_Tests
    {
        [Test]
        public async Task AddFareToCar_EventTriggerTest()
        {
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var mockedFareService = new Mock<IFareService>();

            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object, mockedFareService.Object);

            var car = new TaxiCar("", "");

            var existingFare = new Fare("", "", 1, 1, DateTime.Now.AddDays(-1), car);
            var newFare = new Fare("", "", 1, 9999999, DateTime.Now, car);

            mockedFareService
                .Setup(service => service.GetAllFares())
                .ReturnsAsync(new List<Fare> { existingFare });

            mockedTaxiRepository
                .Setup(repo => repo.Add(It.IsAny<Fare>()))
                .ReturnsAsync(1);

            Fare detectedFare = null;
            mockedFareService
                .Setup(service => service.CheckForHighPaidAmount(It.IsAny<Fare>(), It.IsAny<IEnumerable<Fare>>()))
                .Callback<Fare, IEnumerable<Fare>>((fare, fares) =>
                {
                    if (fare.PaidAmount > fares.Max(f => f.PaidAmount) * 2)
                    {
                        detectedFare = fare;
                    }
                });

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
