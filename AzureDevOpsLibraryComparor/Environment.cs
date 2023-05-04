public class Environment
{
    public const string patKey = "AzureDevOps_PAT";
    public const string baseUriKey = "AzureDevOps_Base";
    private static Environment instance = null;
    private static string? _pat;
    private static string? _baseUri;
    private Environment()
    {

    }

    public static Environment Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Environment();
            }
            return instance;
        }
    }

    public static string? UpdatePat(bool shouldBeRenewed = false)
    {
        Console.WriteLine(
            shouldBeRenewed
                ? "PAT has expired. Please renew PAT and insert here:"
                : "Please insert your PAT here:");
        _pat = Console.ReadLine();
        System.Environment.SetEnvironmentVariable(patKey, _pat, EnvironmentVariableTarget.Machine);
        Console.WriteLine("To effectively store the PAT in you environment variables, you will have to restart this prompt.");
        return _pat;
    }

    public static string? UpdateBaseUri()
    {
        Console.WriteLine("Please insert your base URI here (e.g. https://dev.azure.com/{YourOrganization}/:");
        _baseUri = Console.ReadLine();
        System.Environment.SetEnvironmentVariable(baseUriKey, _baseUri, EnvironmentVariableTarget.Machine);
        Console.WriteLine("To effectively store the base URI to your environment variables, you will have to restart this prompt.");
        return _baseUri;
    }

    public static string? GetPat()
    {
        if (string.IsNullOrEmpty(_pat))
        {
            _pat = System.Environment.GetEnvironmentVariable(patKey, EnvironmentVariableTarget.Machine);
        }

        if (string.IsNullOrWhiteSpace(_pat))
        {
            _pat = UpdatePat();
        }

        return _pat;
    }

    public static string? GetBaseUri()
    {
        if (string.IsNullOrEmpty(_baseUri))
        {
            _baseUri = System.Environment.GetEnvironmentVariable(baseUriKey, EnvironmentVariableTarget.Machine);
        }
       
        if (string.IsNullOrWhiteSpace(_baseUri))
        {
            _baseUri = UpdateBaseUri();
        }

        return _baseUri;
    }
}