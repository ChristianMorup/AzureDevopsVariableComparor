public static class ConsoleEx
{
    public static void WriteMenu(string message, List<Option> options, Option selectedOption, bool shouldClear = true) {
        if (shouldClear)
        {
            Console.Clear();
        }
        Console.WriteLine(message);

        foreach (Option option in options)
        {
            Console.Write(option == selectedOption ? "> " : " ");
            Console.WriteLine(option.Name);
        }
    }

    public static void GetUserInput(ref int i, List<Option> list, string message, Func<bool>? preProcess = null)
    {
        ConsoleKeyInfo consoleKeyInfo;
        do
        {
            consoleKeyInfo = Console.ReadKey();

            // Handle each key input (down arrow will write the menu again with a different selected item)
            if (consoleKeyInfo.Key == ConsoleKey.DownArrow)
            {
                if (i + 1 < list.Count)
                {
                    i++;
                    preProcess?.Invoke();
                    WriteMenu(message, list, list[i], shouldClear: preProcess == null);
                }
            }

            if (consoleKeyInfo.Key == ConsoleKey.UpArrow)
            {
                if (i - 1 >= 0)
                {
                    i--;
                    preProcess?.Invoke();
                    WriteMenu(message, list, list[i], shouldClear: preProcess == null);
                }
            }

            // Handle different action for the option
            if (consoleKeyInfo.Key == ConsoleKey.Enter)
            {
                list[i].Selected.Invoke();
                i = 0;
                break;
            }
        } while (consoleKeyInfo.Key != ConsoleKey.X);
    }

    public static void WriteSettingsReport(List<ExistingSetting> existingSettings, List<RedundantSetting> redundantSettings, List<DiscrepantSetting> discrepantSettings, List<MissingSetting> missingSettings)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"A total of {existingSettings.Count} settings already exists in both environments.");
        Console.WriteLine("The following settings already exists:");
        foreach (var setting in existingSettings)
        {
            Console.WriteLine($"\t{setting.Key}");
        }

        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine($"A total of {redundantSettings.Count} settings are potentially redundant.");
        Console.WriteLine("The following settings are potentially redundant:");
        foreach (var setting in redundantSettings)
        {
            Console.WriteLine($"\t{setting.Key}");
        }

        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"A total of {discrepantSettings.Count} settings have different values in the two environments.");
        Console.WriteLine("The following settings have different values:");
        foreach (var setting in discrepantSettings)
        {
            Console.WriteLine($"\t{setting.Key}");
        }

        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"A total of {missingSettings.Count} settings are missing.");
        Console.WriteLine("The following settings are missing:");
        foreach (var setting in missingSettings)
        {
            Console.WriteLine($"\t{setting.Key}");
        }

        Console.ResetColor();
    }
}