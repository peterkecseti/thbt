using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace UJP6TH_HSZF_2024251.Model
{
    [Table("Fares")]
    public class Fare
    {
        public Fare(string from, string to, int distance, int paidAmount, DateTime fareStartDate, TaxiCar taxiCar)
        {
            From = from;
            To = to;
            Distance = distance;
            PaidAmount = paidAmount;
            FareStartDate = fareStartDate;
            TaxiCar = taxiCar;
        }

        public Fare() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid FareID { get; set; }
        public Guid TaxiID { get; set; }
        [StringLength(100)]
        public string From { get; set; }
        [StringLength(100)]
        public string To { get; set; }
        public int PaidAmount { get; set; }
        public int Distance { get; set; }
        public DateTime FareStartDate { get; set; }
        public virtual TaxiCar TaxiCar { get; set; }
    }
}
