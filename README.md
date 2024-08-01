# Comapanies House Gateway

A simple API for testing the companies house API.

You will need a companies house account. See https://developer.company-information.service.gov.uk/get-started.

Create a REST API application and copy the token then paste it into your config. You can add an `appsettings.Development.json` with the following content:

```
{
  "CompaniesHouseApi": {
    "ApiKey": "<your-api-key>"
  }
}
```

Call the API with the following GET url, with the last part being a companies house number:

```
https://localhost:7131/CompaniesHouse/companies/06499687
```
