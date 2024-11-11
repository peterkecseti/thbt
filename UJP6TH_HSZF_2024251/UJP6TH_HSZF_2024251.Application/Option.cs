namespace UJP6TH_HSZF_2024251.Application
{
    public class Option
    {
        public string OptionName { get; }
        public Func<Task> ActionAsync { get; }

        public Option(string optionName, Func<Task> action)
        {
            OptionName = optionName ?? throw new ArgumentNullException(nameof(optionName));
            ActionAsync = action ?? throw new ArgumentNullException(nameof(action));
        }
        public override string ToString() => OptionName;
    }
}