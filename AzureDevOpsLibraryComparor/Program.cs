using AzureDevOpsLibraryComparor;
using Spectre.Console;
using System.Collections.Generic;

var devOpsService = new AzureDevOpsService();
var projects = await devOpsService.GetProjects();
var expandedColumns = new HashSet<int>();

var project = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("What project do you wish to compare Library Values for?")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more projects)[/]")
        .AddChoices(projects));

var libraries = await devOpsService.GetLibraries(project);


var filteredLibs = FilterLibraries(out var filter, libraries);

if (filteredLibs?.Any() is false)
{
    Console.WriteLine("No libraries found");
    Console.ReadLine();
    return;
}

var chosenLibs = AnsiConsole.Prompt(
    new MultiSelectionPrompt<string>()
        .Title($"Filter: {filter} \nSelect Libraries to compare")
        .PageSize(100)
        .MoreChoicesText("[grey](Move up and down to reveal more libraries)[/]")
        .InstructionsText(
            "[grey](Press [blue]<space>[/] to toggle a library, " +
            "[green]<enter>[/] to accept)[/]")
        .AddChoices(filteredLibs!));


var selectedLibs = libraries.Where(k => chosenLibs.Contains(k.Key)).ToDictionary(k => k.Key, k => k.Value);

var allKeys = selectedLibs.SelectMany(k => k.Value.Keys).ToHashSet();

while (true)
{
    WriteTable(selectedLibs, allKeys, expandedColumns);


    var info = Console.ReadKey();

    if (info.Key == ConsoleKey.Escape)
    {
        break;
    }

    if (info.Key == ConsoleKey.A)
    {
        if (expandedColumns.Count == selectedLibs.Keys.Count)
        {
            expandedColumns = new HashSet<int>();
        }
        else
        {
            for (var j = 0; j < selectedLibs.Keys.Count; j++)
            {
                expandedColumns.Add(j);
            }
        }
    }
    else if (char.IsNumber(info.KeyChar))
    {
        var number = int.Parse(info.KeyChar.ToString());

        if (number < selectedLibs.Keys.Count)
        {
            if (expandedColumns.Contains(number))
            {
                expandedColumns.Remove(number);
            }
            else
            {
                expandedColumns.Add(number);
            }
        }
    }

    Console.Clear();
}

List<string> FilterLibraries(out string filter1, Dictionary<string, Dictionary<string, string>> libraries)
{
    filter1 = "";
    var list = libraries.Keys.Select(k => k).ToList();



    WriteFilterResult(filter1, list, libraries);

    while (true)
    {
        Console.CursorVisible = false;
        var consoleKeyInfo = Console.ReadKey();

        Console.Clear();

        switch (consoleKeyInfo.Key)
        {
            case ConsoleKey.Enter:
                return list;
            case ConsoleKey.Escape:
                return list;
            case ConsoleKey.Backspace:
            {
                if (filter1.Length > 0)
                {
                    filter1 = filter1.Remove(filter1.Length - 1, 1);
                }
                break;
            }
            default:
            {
                if (char.IsLetterOrDigit(consoleKeyInfo.KeyChar) || char.IsSymbol(consoleKeyInfo.KeyChar))
                {
                    filter1 += consoleKeyInfo.KeyChar;
                }
                break;
            }
        }

        if (filter1 != "")
        {
            var s = filter1;
            list = libraries.Keys.Select(k => k.ToLowerInvariant())
                .Where(k => k.Contains(s.ToLowerInvariant()))
                .Select(k => k).ToList();
        }

        WriteFilterResult(filter1, list, libraries);
    }

    return list;
}


void WriteTable(Dictionary<string, Dictionary<string, string>> dictionary, HashSet<string> hashSet, HashSet<int> ints)
{
    var table = new Table();

    table.AddColumn("Key");

    foreach (var lib in dictionary)
    {
        table.AddColumn(lib.Key);
    }
    
    foreach (var key in hashSet)
    {
        var row = new List<string> { key };
        var libraryValues = dictionary.Values.Select(val => val.TryGetValue(key, out var value) ? value : "null").ToList();

        string colorTag;
        if (libraryValues.Any(s => s == "null"))
        {
            colorTag = "[red]";
        }
        else if (libraryValues.Distinct().Count() == 1)
        {
            colorTag = "[green]";
        }
        else
        {
            colorTag = "[yellow]";
        }

        row.AddRange(TruncateAndColor(libraryValues, colorTag, ints));
        table.AddRow(row.ToArray());
    }
    

    var caption = "[grey]Press 'esc' to exit\nPress 'a' to expand all columns\n";

    for (int j = 0; j < dictionary.Keys.Count; j++)
    {
        caption += $"Press {j} to expand values for {dictionary.Keys.ToList()[j]}\n";
    }

    caption += "[/]";

    table.Caption = new TableTitle(caption);

    // Render the table to the console
    AnsiConsole.Write(table);
}

List<string> Truncate(List<string> list, HashSet<int> ints1)
{
    var rowValues1 = new List<string>();

    for (int j = 0; j < list.Count; j++)
    {
        if (ints1.Contains(j) is false)
        {
            rowValues1.Add(list[j].Truncate(4) + "...");
        }
        else
        {
            rowValues1.Add(list[j]);
        }
    }

    return rowValues1;
}

IEnumerable<string> TruncateAndColor(List<string> values, string colorTag, HashSet<int> ints)
{
    return Truncate(values, ints).Select(s => $"{colorTag}{s}[/]");
}

void WriteFilterResult(string filter2, List<string> list1, Dictionary<string, Dictionary<string, string>> dictionary)
{
    AnsiConsole.Write($"Enter filter: {filter2}_ \n");
    AnsiConsole.WriteLine("(pres enter to continue)");

    var n = 0;

    AnsiConsole.WriteLine("\n");
    AnsiConsole.WriteLine("Filtered libraries:");

    foreach (var lib in list1)
    {
        n++;
        AnsiConsole.WriteLine($"  [ ] {lib}");

        if (n > 11)
        {
            AnsiConsole.WriteLine("[grey](More libraries...)[/]");
            break;
        }
    }
}