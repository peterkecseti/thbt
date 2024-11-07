using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UJP6TH_HSZF_2024251.Application
{
    public class Option
    {
        public string OptionName;
        public Action action;
        public Func<string, int> func;

        public Option(string optionName, Action selected)
        {
            this.OptionName = optionName;
            this.action = selected;
        }

        public override string ToString() => OptionName;
    }
}
