﻿namespace UJP6TH_HSZF_2024251.Application
{
    public class SearchOption
    {
        public Func<int> func;
        public string name;

        public SearchOption(string optionName, Func<int> func)
        {
            this.name = optionName;
            this.func = func;
        }

        public override string ToString() => name;
    }
}
