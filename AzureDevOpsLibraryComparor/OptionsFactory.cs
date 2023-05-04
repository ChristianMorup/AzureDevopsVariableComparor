public class OptionsFactory
{
    public static List<Option> CreateOptions(List<string> list, Func<string, bool> action)
    {
        List<Option> options = new List<Option>();
        foreach (var s in list)
        {
            options.Add(new Option(s, () => action(s)));
        }

        return options;
    }
}