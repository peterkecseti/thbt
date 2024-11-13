using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UJP6TH_HSZF_2024251.Application
{
    public class LicensePlateException : Exception
    {
        public LicensePlateException(string message = "[red]Ez a rendszám már használatban van![/]") : base(message) { }
    }

    public class BadJsonException : Exception
    {
        public BadJsonException(string message = "[red]Nem megfelelő JSON formátum![/]") : base(message) { }
    }
}
