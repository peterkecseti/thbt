using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UJP6TH_HSZF_2024251.Model.Entities
{
    [Table("TaxiCars")]
    public class TaxiCar
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TaxiID { get; set; }

        [StringLength(50)]
        public string LicensePlate { get; set; }

        [StringLength(100)]
        public string Driver { get; set; }
        public virtual ICollection<Fare> Fares { get; set; }

        public TaxiCar(string licensePlate, string driver)
        {
            LicensePlate = licensePlate;
            Driver = driver;
            Fares = new List<Fare>();
        }

        public TaxiCar(string licensePlate, string driver, List<Fare> fares)
        {
            LicensePlate = licensePlate;
            Driver = driver;
            Fares = fares;
        }
        public override string ToString() => $"{LicensePlate} ({Driver})";
    }
}
