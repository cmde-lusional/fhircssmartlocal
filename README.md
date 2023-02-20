# fhircssmartlocal

## OAuth Connection 

OAuth attributes are set as an extension of the metadata which is a capabilitystatement resource. 

The specification for the oauth extension can be found here: 

[oauth](https://fhir-ru.github.io/extension-oauth-uris.html)

The following command can be used in cli to receive the oauth "*authorized*" and "*token*" of the oauth extension:
```
{
    curl -v "https://launch.smarthealthit.org/v/r4/sim/WzIsIiIsImU0NDNhYzU4LThlY2UtNDM4NS04ZDU1LTc3NWMxYjhmM2EzNyIsIkFVVE8iLDAsMCwwLCIiLCIiLCIiLCIiLCIiLCIiLCIiLDAsMV0/fhir/metadata" | grep -B 10 -A 10 StructureDefinition/oauth-uris
}
```

An extension only holds an url and a value. There is only one value. The trick is to nest multiple extension into one extension . In our case by using a list of extensions inside the value. In our case this list holds oauth extension like *authorized* and *token*. 

The following output displays the structure:

```
{
    "rest": [
        {
            "mode": "server",
            "security": {
                "cors": true,
                "extension": [
                    {
                        "url": "http://fhir-registry.smarthealthit.org/StructureDefinition/oauth-uris",
                        "extension": [
                            {
                                "url": "authorize",
                                "valueUri": "https://launch.smarthealthit.org/v/r4/sim/WzIsIiIsImU0NDNhYzU4LThlY2UtNDM4NS04ZDU1LTc3NWMxYjhmM2EzNyIsIkFVVE8iLDAsMCwwLCIiLCIiLCIiLCIiLCIiLCIiLCIiLDAsMV0/auth/authorize"
                            },
                            {
                                "url": "token",
                                "valueUri": "https://launch.smarthealthit.org/v/r4/sim/WzIsIiIsImU0NDNhYzU4LThlY2UtNDM4NS04ZDU1LTc3NWMxYjhmM2EzNyIsIkFVVE8iLDAsMCwwLCIiLCIiLCIiLCIiLCIiLCIiLCIiLDAsMV0/auth/token"
                            },
}
```

## Web Server

To setup the webserver it can be helpful to use some code from the microsoft dotnet web package. One could create a temporary folder and install the 
packe inside as follows

```
{
    dotnet new web
}
```

Following code is to be copied

```
{
    cp appsettings.json ../fhircssmartlocal/fhircssmartlocal/
}
```

### Adding a dynamic web server url while picking a free port 
The tutorial is using an older version of the webapp package of dotnet. In the newer version we the code is by far shorter.
In the tutorial urls are changed to avoid complications with other apps while starting the app.
Therefore the author is changing inside the program.cs the CreateHostBuilder function and adds two attributes:

- webBuilder.UseUrls = this changes the url of the server where the :0 lets the program pick a free port
- webBuilder.UseKestrel = not necessary because the package is automatically using kestrel

The webBuilder.UseUrls can be changed to:

```
{
    builder.WebHost.UseUrls("http://127.0.0.1:0");
}
```

### Retrieving a list of the server urls

Because we let the program decide which port is to be picked we cannot statically use this address inside our code, so we need to ask the program
to provide us the url:

```
{
//print the urls that have been picked by the UseUrls function above 
Console.WriteLine("URLs:");
int urlCounter = 0;
foreach (var weburl in app.Urls)
{
    Console.WriteLine($"url {urlCounter, 3}: {weburl}");
}
}
```

The above code shows how to extract the urls. Note that this code needs to be placed after starting the app with app.Start().

The code below is an alternative to get the port number while using the Webhost.Webapplicaton

```
{
while (!urlsAvailable && count < 10)
{
    //as soon as there is an entry in app.Urls
    if (app.Urls.Any())
    {
        urlsAvailable = true;
        Console.WriteLine(app.Urls.ToString());
        
        int i = 0;
        foreach (string weburl in app.Urls)
        {
            Console.WriteLine(weburl[i]);
            try
            {
                if (string.IsNullOrEmpty(weburl))
                {
                    continue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            try
            {
                if (weburl.Length < 18)
                {
                    continue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            try
            {
                if (int.TryParse(weburl.Substring(17), out listenPort) && listenPort != 0) ;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            i++;

        }
        
    }
    else
    {
        Console.WriteLine("app.Urls is empty, waiting for a few seconds...");
        Thread.Sleep(2000);
        count++;
    }
}
}
```


### Deserializing the Access Token json response

During the last step the server response with an access token in json format:

```
{
{
    "access_token":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzY29wZSI6Im9wZW5pZCBmaGlyVXNlciBwcm9maWxlIGxhdW5jaC9wYXRpZW50IHBhdGllbnQvKi5yZWFkIiwiaWF0IjoxNjc2ODU5MjcyLCJleHAiOjE2NzY4NjI4NzJ9.6IbTcZtd9TxHNABXNsHlmUGOSldQ9SBexbsUn4dqggE",
    "token_type":"Bearer",
    "expires_in":3600,
    "scope":"openid fhirUser profile launch/patient patient/*.read",
    "id_token":"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcm9maWxlIjoiUHJhY3RpdGlvbmVyL2U0NDNhYzU4LThlY2UtNDM4NS04ZDU1LTc3NWMxYjhmM2EzNyIsImZoaXJVc2VyIjoiUHJhY3RpdGlvbmVyL2U0NDNhYzU4LThlY2UtNDM4NS04ZDU1LTc3NWMxYjhmM2EzNyIsImF1ZCI6ImZoaXJfZGVtb19pZCIsInN1YiI6ImY2M2M1ZTlhMDBkZDQ4NzhiNzM0ZTYwZTBhNjQ2NzdiYzI3YjgxOGQ4YTNmYjk4MTNhZDJmMzUyMzZkYWQyZWEiLCJpc3MiOiJodHRwczovL2xhdW5jaC5zbWFydGhlYWx0aGl0Lm9yZy92L3I0L3NpbS9XeklzSWlJc0ltVTBORE5oWXpVNExUaGxZMlV0TkRNNE5TMDRaRFUxTFRjM05XTXhZamhtTTJFek55SXNJa0ZWVkU4aUxEQXNNQ3d3TENJaUxDSWlMQ0lpTENJaUxDSWlMQ0lpTENJaUxEQXNNVjAvZmhpciIsImlhdCI6MTY3Njg1OTI3MiwiZXhwIjoxNjc2ODYyODcyfQ.kSNDDgiucBmy9-8GPwkofwBFZVC1tNwfzRodz1Vc3TXwHQm_ii4dBewa8qBCOXu2HN_VCrcVzuamjG8T9WYU33SR-WUxn0MU4FvlayFqQDAmv2hqLOpObh1V9mbrt0irGmjsHagfHSu2jxFswhY3HPWsqlG0wDc9dCFJh5G89XCUuVBwLlMEAjl_j7YIgJUSTdW0RBjsWEP2e5sI6AA0O6gqK98Ga7--kP31VORUJRv2RpoIOjc-62HGY_VLoHp9yPFDU188CJ5MVBMtPuF4rOTjctw8qxUYeCIUkIHYb6VWD9coHKrG5dAeBkEppT7Mii9YttTt9q1Az_EE5laisw",
    "need_patient_banner":true,
    "smart_style_url":"https://launch.smarthealthit.org/smart-style.json",
    "patient":"80a75b5a-fd30-4f38-895d-d8098fe7206e"
}
}
```

This needs to be parsed by our program. You can find the deserilization process under SmartResponse.cs

By using the "System.Text.Json.Serialization" package we can define "get" and "set" methods inside the class as follows:

```
{
    //class to deserialize smart auth responses
    public class SmartResponse
    {
        [JsonPropertyName("need_patient_banner")]
        public bool needPatientBanner { get; set; }

        ...

}
```

ERROR: fhirClient.OnBeforeRequest doesn't exist. 

Because we want to add the Bearer token to the FhirClient request, we need to add a new HttpClient with an HttpClientHandler and change the "Authorization" attribute of the handler to the value from our smartResponse.accesstoken to provide the access token to the FhirServer. In the end we can read the patient.
All this is done in our "doSomethingWithToken" function.

Solution:

```
{
    using var httpClientHandler = new HttpClientHandler();
    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

    HttpClient httpClient = new HttpClient(httpClientHandler);
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", smartResponse.accessToken);

    FhirClient fhirClient = new FhirClient(fhirServerUrl, httpClient, settings);
}
```