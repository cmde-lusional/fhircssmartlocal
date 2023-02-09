// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using fhircssmartlocal;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;


Console.WriteLine("Hello, World!");

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
