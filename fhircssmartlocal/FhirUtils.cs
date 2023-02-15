using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Builder;

namespace fhircssmartlocal;

//FHIR Utility functions
public class FhirUtils
{

    public static bool launchUrl(string url)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = url,
                UseShellExecute = true,
            };
        
            Process.Start(startInfo);
        }
    
        catch (Exception)
        {
            Console.WriteLine($"Failed to launch URL");
            return false;
        }
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                return true;
            }
            catch (Exception)
            {
                // ignore
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] allowedProgramsToRun = { "xdg-open", "gnome-open", "kfmclient" };

            foreach (string helper in allowedProgramsToRun)
            {
                try
                {
                    Process.Start(helper, url);
                    return true;
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                Process.Start("open", url);
                return true;
            }
            catch (Exception)
            {
                // ignore
            }
        }

        System.Console.WriteLine($"Failed to launch URL");
        return false;
    }

    //How to get Smart OAuth access urls
    public static bool TryGetSmartUrls(FhirClient fhirClient, out string authorizeUrl, out string tokenUrl)
    {
        authorizeUrl = string.Empty;
        tokenUrl = string.Empty;
        /*The oauth urls are included inside the metadata. Because metadata is a capability we need to start get the
         capabilityStatement of the metadata.*/
        CapabilityStatement capabilities = (CapabilityStatement)fhirClient.Get("metadata");
        
        /*Next Step is to get the Rest informations in form multiple restComponent of the capabilityStatment
         (loop over these components)*/
        foreach (CapabilityStatement.RestComponent restComponent in capabilities.Rest)
        {
            /*The information are under restComponent.Security, if there is no entry then skip this rest component*/
            if (restComponent.Security == null)
            {
                Console.WriteLine("no security element found");
            }
            /*The can be multiple security extension inside the security extension, some of them might not include OAuth
             informations*/
            foreach (Extension securityExtension in restComponent.Security.Extension)
            {   
                /*if the extension is not holding oauth uris then skip?*/
                if (securityExtension.Url != "http://fhir-registry.smarthealthit.org/StructureDefinition/oauth-uris")
                {
                    continue;
                }

                if (securityExtension.Extension == null || securityExtension.Extension.Count == 0)
                {
                    continue;
                }
                
                /*the extension informations are nested extension so we need to search further inside the
                 security.extension. you could say we are in security.extension.extension*/
                foreach (Extension smartExtension in securityExtension.Extension)
                {
                    switch (smartExtension.Url)
                    {
                        case "authorize":
                            authorizeUrl = smartExtension.Value.ToString();
                            break;
                        case "token":
                            tokenUrl = smartExtension.Value.ToString();
                            break;
                    }
                }
            }
            
        }

        if (string.IsNullOrEmpty(authorizeUrl) || string.IsNullOrEmpty(tokenUrl))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}

