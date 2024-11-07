using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UJP6TH_HSZF_2024251.Model
{
    [Table("TaxiCars")]
    public class TaxiCar
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TaxiID { get; set; }

        [StringLength(50)]
        public  string LicensePlate { get; set; }

        [StringLength(100)]
        public  string Driver {  get; set; }
        public virtual ICollection<Fare> Fares { get; set; }

        public TaxiCar(string licensePlate, string driver) {
            this.LicensePlate = licensePlate;
            this.Driver = driver;
            this.Fares = new List<Fare>();
        }

        public TaxiCar(string licensePlate, string driver, List<Fare> fares) {
            this.LicensePlate = licensePlate;
            this.Driver = driver;
            this.Fares = fares;
        }


        public override string ToString() => $"{LicensePlate} ({Driver})";
    }
}
