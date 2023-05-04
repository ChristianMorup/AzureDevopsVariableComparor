using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

public class AzureDevOpsService
{
    private HashSet<string> _libraryCategories = new() { "IaC", "BC", "Adpt", "Others" };
    private HashSet<string> _environments = new() { "Test", "Acceptance", "Production" };
    private string _pat;
    private string _baseUri;
    private Dictionary<string, Dictionary<string, string>>? _libraries;

    public AzureDevOpsService()
    {
        _pat = Environment.GetPat() ?? throw new NullReferenceException();
        _baseUri = Environment.GetBaseUri() ?? throw new NullReferenceException();
    }

    public async Task<string> GetProject()
    {
        var projects = await GetProjects();
        var project = projects[0];

        List<Option> options = OptionsFactory.CreateOptions(projects, (s) => {
            project = s;
            return true;
        });

        int index = 0;
        ConsoleEx.WriteMenu("Choose project", options, options[index]);
        ConsoleEx.GetUserInput(ref index, options, "Choose project");

        return project;
    }

    public Tuple<string, string> GetEnvironmentsToCompare()
    {
        int firstIndex = 0;
        var firstEnvironment = _environments.First();
        List<Option> firstOptions = OptionsFactory.CreateOptions(_environments.ToList(), (s) => {
            firstEnvironment = s;
            return true;
        });

        ConsoleEx.WriteMenu("Choose the first environment to compare", firstOptions, firstOptions[firstIndex]);
        ConsoleEx.GetUserInput(ref firstIndex, firstOptions, "Choose the first environment to compare");


        var environments = _environments.Select(s => s).ToHashSet();
        environments.Remove(firstEnvironment);
        var secondEnvironment = environments.First();
        List<Option> secondOptions = OptionsFactory.CreateOptions(environments.ToList(), (s) => {
            secondEnvironment = s;
            return true;
        });

        Console.Clear();
        ConsoleEx.WriteMenu("Choose the first environment to compare", firstOptions, firstOptions[_environments.ToList().IndexOf(firstEnvironment)]);

        int secondIndex = 0;
        ConsoleEx.WriteMenu($"What environment do you want to compare {firstEnvironment} with?", secondOptions, secondOptions[secondIndex], false);
        ConsoleEx.GetUserInput(ref secondIndex, secondOptions,
            $"What environment do you want to compare {firstEnvironment} with?", preProcess:
            () =>
            {
                Console.Clear();
                ConsoleEx.WriteMenu("Choose the first environment to compare", firstOptions, firstOptions[_environments.ToList().IndexOf(firstEnvironment)]);
                return true;
            });

        return new Tuple<string, string>(firstEnvironment, secondEnvironment);
    }

    public string GetService(HashSet<string> services)
    {
        string service = "";
        var options = services.Select(so => new Option(so, () => service = so)).ToList();
        options.Add(new Option("Exit", () => System.Environment.Exit(0)));
        int index = 0;
        ConsoleEx.WriteMenu("Choose the library you want to compare settings for:", options, options[index]);
        ConsoleEx.GetUserInput(ref index, options, "Choose the library you want to compare settings for:");

        return service;
    }

    public void WriteResults(string service, Tuple<string, string> environments, Dictionary<string, Dictionary<string, string>> libraries)
    {
        var envs = new HashSet<string>() { environments.Item1, environments.Item2 };

        var testLibraryName = service.Replace("{env}", "t");
        var acceptLibraryName = service.Replace("{env}", "a");
        var productionLibraryName = service.Replace("{env}", "p");

        var testLibrary = libraries[testLibraryName];
        var acceptLibrary = libraries[acceptLibraryName];
        var productionLibrary = libraries[productionLibraryName];

        Dictionary<string, string>? firstLib = null;
        Dictionary<string, string>? secondLib = null;

        if (envs.Contains("Test"))
        {
            firstLib = testLibrary;
        } 
        
        if (envs.Contains("Acceptance"))
        {
            if (firstLib == null) { firstLib = acceptLibrary; }
            else
            {
                secondLib = acceptLibrary;
            }
        }

        if (envs.Contains("Production"))
        {
            secondLib = productionLibrary;
        }

        CheckDictionaries(firstLib!, secondLib!);
    }


    static void CheckDictionaries(Dictionary<string, string> test, Dictionary<string, string> accept)
    {
        // Get the keys of both dictionaries
        var testKeys = test.Keys.ToHashSet();
        var acceptKeys = accept.Keys.ToHashSet();

        var allKeys = testKeys.Select(a => a).ToHashSet();
        allKeys.UnionWith(acceptKeys);

        var missingSettings = new List<MissingSetting>();
        var redundantSettings = new List<RedundantSetting>();
        var discrepantSettings = new List<DiscrepantSetting>();
        var existingSettings = new List<ExistingSetting>();

        foreach (var key in allKeys)
        {
            if (acceptKeys.Contains(key) is false)
            {
                missingSettings.Add(new MissingSetting()
                {
                    Key = key
                });
            }
            else if (testKeys.Contains(key) is false)
            {
                redundantSettings.Add(new RedundantSetting()
                {
                    Key = key
                });
            }
            else if (test[key] == accept[key])
            {
                existingSettings.Add(new ExistingSetting()
                {
                    Key = key
                });
            }
            else if (test[key] != accept[key])
            {
                discrepantSettings.Add(new DiscrepantSetting()
                {
                    Key = key,
                    AcceptValue = accept[key],
                    TestValue = test[key]
                });
            }
        }

        ConsoleEx.WriteSettingsReport(existingSettings, redundantSettings, discrepantSettings, missingSettings);
    }

    public HashSet<string> GetServicesToSelectFrom(Dictionary<string, Dictionary<string, string>> libraries)
    {
        var type = _libraryCategories.First();
        List<Option> options = OptionsFactory.CreateOptions(_libraryCategories.ToList(), (s) => {
            type = s;
            return true;
        });

        int index = 0;
        ConsoleEx.WriteMenu("Choose between IaC, BC or Adpt", options, options[index]);
        ConsoleEx.GetUserInput(ref index, options, "Choose between IaC, BC or Adpt");

        var services = new HashSet<string>();
        
        foreach (var key in libraries.Keys)
        {
            if (type == "Others")
            {
                if (key.Contains("IaC", StringComparison.CurrentCultureIgnoreCase) is false &&
                    key.Contains("BC", StringComparison.CurrentCultureIgnoreCase) is false &&
                    key.Contains("Adpt", StringComparison.CurrentCultureIgnoreCase) is false)
                {
                    AddServicesToHashset(key, services);
                }
            }
            else
            {
                if (key.Contains(type, StringComparison.CurrentCultureIgnoreCase))
                {
                    AddServicesToHashset(key, services);
                }
            }
        }

        return services;
    }

    private void AddServicesToHashset(string key, HashSet<string> hashSet)
    {
        if ((key.Contains("-a") && key.EndsWith("-a")) ||
            (key.Contains("-t") && key.EndsWith("-t")) ||
            (key.Contains("-p") && key.EndsWith("-p")))
        {
            var service = RemoveFromEnd(key, "-a");
            service += "-{env}";

            if (hashSet.Contains(service) is false)
            {
                hashSet.Add(service);
            }
        }
        else
        {
            var service = key.Replace("-a", "-{env}")
                .Replace("-a-", "-{env}-")
                .Replace("-t", "-{env}")
                .Replace("-t-", "-{env}-")
                .Replace("-p", "-{env}")
                .Replace("-p-", "-{env}-");

            if (service.Contains("{env}") && hashSet.Contains(service) is false)
            {
                hashSet.Add(service);
            }
        }
    }

    private static string RemoveFromEnd(string s, string suffix)
    {
        return s.Substring(0, s.Length - suffix.Length);
    }


    public async Task<Dictionary<string, Dictionary<string, string>>> GetLibraries(string project, bool forceUpdate = false)
    {
        if (_libraries != null && forceUpdate is false)
        {
            return _libraries;
        }

        var variableGroups = await GetVariableGroups(project);

        Dictionary<string, Dictionary<string, string>> dictionary = new();

        foreach (var group in variableGroups)
        {
            var name = group.Name;
            var variables = group.Variables.ToDictionary(g => g.Key, g => g.Value.Value);
            dictionary[name] = variables;
        }

        _libraries = dictionary;
        return _libraries;
    }


    private async Task<List<VariableGroup>> GetVariableGroups(string project, bool patIsRenewed = false)
    {
        try
        {
            var credentials = new VssCredentials(new VssBasicCredential("", _pat));
            var vssConnection = new VssConnection(new Uri(_baseUri), credentials);
            var taskAgentClient = vssConnection.GetClient<TaskAgentHttpClient>();
            var variableGroups = await taskAgentClient.GetVariableGroupsAsync(project: project);
            return variableGroups;
        }
        catch (Exception e)
        {
            if (patIsRenewed)
            {
                Console.WriteLine(e);
                throw;
            }

            _pat = Environment.UpdatePat(true) ?? throw new NullReferenceException();
            return await GetVariableGroups(project, true);
        }
    }

    private async Task<List<string>> GetProjects(bool patIsRenewed = false)
    {
        try
        {
            var credentials = new VssCredentials(new VssBasicCredential("", _pat));
            var vssConnection = new VssConnection(new Uri(_baseUri), credentials);
            var client = vssConnection.GetClient<ProjectHttpClient>();
            var projectReferences = await client.GetProjects();
            var projects = projectReferences.Select(p => p.Name).ToList();
            return projects;
        }
        catch (Exception e)
        {
            if (patIsRenewed)
            {
                Console.WriteLine(e);
                throw;
            }

            _pat = Environment.UpdatePat(true) ?? throw new NullReferenceException();
            return await GetProjects(true);
        }
    }
}