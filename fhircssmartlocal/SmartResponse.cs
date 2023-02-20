using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace fhircssmartlocal;

//class to deserialize smart auth responses
public class SmartResponse
{   
    [JsonPropertyName("access_token")]
    public string accessToken{ get; set; } 
    
    [JsonPropertyName("token_type")]
    public string tokenType { get; set; } 
    
    [JsonPropertyName("expires_in")]
    public int expireIn{ get; set; } 
    
    [JsonPropertyName("scope")]
    public string scope{ get; set; } 
    
    [JsonPropertyName("id_token")]
    public string idToken{ get; set; } 
    
    [JsonPropertyName("need_patient_banner")]
    public bool needPatientBanner { get; set; }
    
    [JsonPropertyName("smart_style_url")]
    public string smartStyleUrl { get; set; } 
    
    [JsonPropertyName("patient")]
    public string patientId { get; set; }
    
    /*[JsonPropertyName("client_id")]
    public string clientId{ get; set; } 
    
    [JsonPropertyName("refresh_token")]
    public string refreshToken{ get; set; } */
    
}

