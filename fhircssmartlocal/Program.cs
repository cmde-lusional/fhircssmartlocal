// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading.Channels;
using fhircssmartlocal;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
using Task = System.Threading.Tasks.Task;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//Setting a dynamic web server url while picking a free port number
builder.WebHost.UseUrls("http://127.0.0.1:0");

app.MapGet("/", () => "Hello World!");

/*startWebserverInBackground();*/

app.Start();

//print the urls that have been picked by the UseUrls function above 
Console.WriteLine("URLs:");

int port = 0;

int urlCounter = 0;
foreach (var weburl in app.Urls)
{
    if (string.IsNullOrEmpty(weburl))
    {
        continue;
    }

    if (weburl.Length < 18)
    {
        continue;
    }

    Console.WriteLine($"url {urlCounter, 3}: {weburl}");

    if (int.TryParse(weburl.Substring(17), out port) && port != 0);

    Console.WriteLine($"Port: {port}");
}

/*for (int loops = 0; loops > 10; loops++)
{
    System.Threading.Thread.Sleep(100000000);
}

async void startWebserverInBackground()
{
    Task.Run(() => app.Run());
    await Task.Delay(500);
}*/

// connection dic for fire server
Dictionary<string, string> _fhirServers = new Dictionary<string, string>()
{
    { "Smart", "https://launch.smarthealthit.org/v/r4/sim/WzIsIiIsImU0NDNhYzU4LThlY2UtNDM4NS04ZDU1LTc3NWMxYjhmM2EzNyIsIkFVVE8iLDAsMCwwLCIiLCIiLCIiLCIiLCIiLCIiLCIiLDAsMV0/fhir" },
};

// read in settings for following fire server connection
var settings = new FhirClientSettings
{
    PreferredFormat = ResourceFormat.Json,
    PreferredReturn = Prefer.ReturnRepresentation
};

string _fhirServer = _fhirServers["Smart"];

Hl7.Fhir.Rest.FhirClient fhirClient = new Hl7.Fhir.Rest.FhirClient(_fhirServer, settings);

if (!FhirUtils.TryGetSmartUrls(fhirClient, out string authorizeUrl, out string tokenUrl))
{
    Console.WriteLine("Failed to discover smart configuration");
    return -1;
}
else
{
    Console.WriteLine($"authorized url: {authorizeUrl}");
    Console.WriteLine($"token url: {tokenUrl}");
    return 0;
}


