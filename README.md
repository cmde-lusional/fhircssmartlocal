# fhircssmartlocal

## OAuth Connection 

OAuth attributes are set as an extension of the metadata which is a capabilitystatement resource. 

The specification for the oauth extension can be found here: 

[title](https://fhir-ru.github.io/extension-oauth-uris.html)

The following command can be used in cli to receive the oauth "*authorized*" and "*token*" of the oauth extension:
```
{
    curl -v "https://launch.smarthealthit.org/v/r4/sim/WzIsIiIsImU0NDNhYzU4LThlY2UtNDM4NS04ZDU1LTc3NWMxYjhmM2EzNyIsIkFVVE8iLDAsMCwwLCIiLCIiLCIiLCIiLCIiLCIiLCIiLDAsMV0/fhir/metadata" | grep -A 10 StructureDefinition/oauth-uris
}
```

An extension only holds an url and a value. There is only one value. The trick is to nest multiple extension into one extension . In our case by using a list of extensions inside the value. In our case this list holds oauth extension like *authorized* and *token*. 


