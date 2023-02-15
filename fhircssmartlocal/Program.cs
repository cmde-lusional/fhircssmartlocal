// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices.ComTypes;
using System.Web;
using fhircssmartlocal;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Task = System.Threading.Tasks.Task;

namespace fhircssmartlocal;

public class Program
{
    private const string _clientId = "fhir_demo_id";
    public static string _tokenUrl = string.Empty;
    public static string _authCode = string.Empty;
    public static string _clientState = string.Empty;
    public static string _redirectUrl = string.Empty;
        
    // connection dic for fire server
    static Dictionary<string, string> _fhirServers = new Dictionary<string, string>()
    {
        { "Smart", "https://launch.smarthealthit.org/v/r4/sim/WzIsIiIsImU0NDNhYzU4LThlY2UtNDM4NS04ZDU1LTc3NWMxYjhmM2EzNyIsIkFVVE8iLDAsMCwwLCIiLCIiLCIiLCIiLCIiLCIiLCIiLDAsMV0/fhir" },
    };

    // read in settings for following fire server connection
    static FhirClientSettings settings = new FhirClientSettings()
    {
        PreferredFormat = ResourceFormat.Json,
        PreferredReturn = Prefer.ReturnRepresentation,
    };

    static string fhirServerUrl = _fhirServers["Smart"];

    static int Main(string[] args)
    {
        Console.WriteLine($"FHIR Server: {fhirServerUrl}");

        FhirClient fhirClient = new FhirClient(fhirServerUrl);

        if (!FhirUtils.TryGetSmartUrls(fhirClient, out string authorizeUrl, out string tokenUrl))
        {
            Console.WriteLine($"Failed to discover SMART URLs");
            return -1;
        }

        Console.WriteLine($"Authorize URL: {authorizeUrl}");
        Console.WriteLine($"    Token URL: {tokenUrl}");
        _tokenUrl = tokenUrl;
            
        //MapGet("/", () => "Hello World!");

        //Run the app in the background: This will run the app.Run() method in a separate background task, allowing your program to continue executing code after the call to StartNew()
        //Problem then is if we want to catch the port number which is randomly choosen by the program we need to wait for the web server to start in the background
        Task.Factory.StartNew(() => {
            CreateHostBuilder().Build().Run();
        });

        int listenPort = GetListenPort().Result;

        Console.WriteLine($"ListenPort: {listenPort}");
                
        Console.WriteLine($" Listening on: {listenPort}");
        _redirectUrl = $"http://127.0.0.1:{listenPort}";

        //Setting up the url for contacting the auth server of FHIR
        string url = 
            $"{authorizeUrl}" + 
            $"?response_type=code" + 
            $"&client_id={_clientId}" +
            $"&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}" +
            $"&scope={HttpUtility.UrlEncode("openid fhirUser profile launch/patient patient/*.read")}" +
            $"&state=local_state" +
            $"&aud={fhirServerUrl}";

        Console.WriteLine($"Url: {url}");

        FhirUtils.launchUrl(url);

        //the following jwt response will look something like this:
        //https://jwt.io/

        //http://127.0.0.1:62276/?code=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb250ZXh0Ijp7Im5lZWRfcGF0aWVudF9iYW5uZXIiOnRydWUsInNtYXJ0X3N0eWxlX3VybCI6Imh0dHBzOi8vbGF1bmNoLnNtYXJ0aGVhbHRoaXQub3JnL3NtYXJ0LXN0eWxlLmpzb24iLCJwYXRpZW50IjoiMmNkYTVhYWQtZTQwOS00MDcwLTlhMTUtZTFjMzVjNDZlZDVhIn0sImNsaWVudF9pZCI6ImZoaXJfZGVtb19pZCIsInJlZGlyZWN0X3VyaSI6Imh0dHA6Ly8xMjcuMC4wLjE6NjIyNzYiLCJzY29wZSI6Im9wZW5pZCBmaGlyVXNlciBwcm9maWxlIGxhdW5jaC9wYXRpZW50IHBhdGllbnQvKi5yZWFkIiwicGtjZSI6ImF1dG8iLCJjbGllbnRfdHlwZSI6InB1YmxpYyIsInVzZXIiOiJQcmFjdGl0aW9uZXIvZTQ0M2FjNTgtOGVjZS00Mzg1LThkNTUtNzc1YzFiOGYzYTM3IiwiaWF0IjoxNjc2Mjc4MTQ2LCJleHAiOjE2NzYyNzg0NDZ9.p1Yw8ErFy9Pf1UDR7TqABdCLh07s4tVESBIzjk6AfOU&state=local_state

        //this line determines how long the web app is running in the background
        for (int loops = 0; loops < 60; loops++)
        {
            Thread.Sleep(1000);
        }
        return 0;
    }
        
    static async Task<int> GetListenPort()
    {
        for (int loops = 0; loops < 100; loops++)
        {
            await Task.Delay(100);
            if (Startup.Addresseses == null)
            {
                continue;
            }

            string address = Startup.Addresseses.Addresses.FirstOrDefault();

            if (string.IsNullOrEmpty(address))
            {
                continue;
            }

            if (address.Length < 18)
            {
                continue;
            }

            if ((int.TryParse(address.Substring(17), out int port)) &&
                (port != 0))
            {
                return port;
            }
        }

        throw new Exception($"Failed to get listen port!");
    }

    public static async void setAuthCode(string code, string state)
    {
        _authCode = code;
        _clientState = state;

        Console.WriteLine($"Code received: {code}");

        Dictionary<string, string> requestValues = new Dictionary<string, string>()
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", _redirectUrl },
            { "client_id", _clientId }
        };

        HttpRequestMessage request = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(_tokenUrl),
            Content = new FormUrlEncodedContent(requestValues)
        };

        HttpClient client = new HttpClient();

        HttpResponseMessage response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to exchange code for token!");
            throw new Exception($"Unauthorized: {response.StatusCode}");
        }

        string json = await response.Content.ReadAsStringAsync();

        Console.WriteLine("TokenTokenTokenToken");
        Console.WriteLine(json);
        Console.WriteLine("TokenTokenTokenToken");
    }
        
    static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://127.0.0.1:0");
                webBuilder.UseKestrel();
                webBuilder.UseStartup<Startup>();
            });

}