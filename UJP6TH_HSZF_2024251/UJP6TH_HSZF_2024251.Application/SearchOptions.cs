namespace UJP6TH_HSZF_2024251.Application
{
    public class SearchOptions
    {
        public Func<int> func;
        public string name;

        // Property for file selector
        public string path;

        public SearchOptions(string optionName, Func<int> func)
        {
            this.name = optionName;
            this.func = func;
        }

        // Constructor for file selector
        public SearchOptions(string optionName, string path)
        {
            this.name = optionName;
            this.path = path;
        }

        public override string ToString() => name;
    }
}
