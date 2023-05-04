using System;

Console.WriteLine("Powering up AwesomeLibraryTool3000");


var devOpsService = new AzureDevOpsService();


var project = await devOpsService.GetProject();

var libraries = await devOpsService.GetLibraries(project);

var services = devOpsService.GetServicesToSelectFrom(libraries);

var environments = devOpsService.GetEnvironmentsToCompare();

var selectedService = devOpsService.GetService(services);

devOpsService.WriteResults(selectedService, environments, libraries);

ConsoleKeyInfo consoleKeyInfo;
do
{
    Console.WriteLine("Pres R for refresh or X for exit");
    consoleKeyInfo = Console.ReadKey();
    if (consoleKeyInfo.Key == ConsoleKey.R)
    {
        libraries = await devOpsService.GetLibraries(project, true);
        devOpsService.WriteResults(selectedService, environments, libraries);
    }
} while(consoleKeyInfo.Key != ConsoleKey.X);

