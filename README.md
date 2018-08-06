# V4DevCouncilJun2018
Intuit V4 GraphQL Sample for .NET

Prerequisites:
1. Visual Studio
2. .NET Framework >= `4.6.1`
3. Intuit Developer App in [pre-production environment](https://developer-stage.intuit.com)
4. QuickBooks Company in [pre-production environment](https://silver-release.qbo.intuit.com)
5. Access to [OAuth2 playground](https://developer-stage.intuit.com/v2/ui#/playground) to generate bearer token

Getting started:
1. Clone or download the code from this repository
2. Launch Visual Studio and double click the `.sln` file to open the solution
3. Restore any NuGet packages that need to be downloaded
4. Replace the temporary bearer token in `Auth/danger-insecure-sample-only.txt`
5. Update any hard-coded IDs in the sample code to match your company's resource IDs
5. Run the project
