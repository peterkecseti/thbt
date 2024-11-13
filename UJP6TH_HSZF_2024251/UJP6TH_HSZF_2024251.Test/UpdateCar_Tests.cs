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
using UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Model.Entities;

namespace UJP6TH_HSZF_2024251.Test
{
    [TestFixture]
    public class UpdateCar_Tests
    {
        [Test]
        public async Task UpdateCar_ChangesLicensePlateIfUnique()
        {
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var mockedTaxiService = new Mock<ITaxiService>();
            var mockedFareService = new Mock<IFareService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object, mockedFareService.Object);
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
            var mockedFareService = new Mock<IFareService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object, mockedFareService.Object);

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
            var mockedFareService = new Mock<IFareService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object, mockedFareService.Object);
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
            var mockedFareService = new Mock<IFareService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object, mockedFareService.Object);
            var taxiCar = new TaxiCar("OLD-PLATE", "gasparlaci");

            await taxiService.UpdateCar(taxiCar);

            mockedTaxiRepository.Verify(ctx => ctx.UpdateCar(taxiCar), Times.Once);
        }
    }
}
