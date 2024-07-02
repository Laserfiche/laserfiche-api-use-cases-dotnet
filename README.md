# lf-sample-repository-api-dotnet-srv

Sample .NET core service app that connects to a Laserfiche Cloud or Self-Hosted Repository.
[Sample Code](./Program.cs)

## Cloud Prerequisites

### Software Prerequisites

- .NET 8.0 core SDK
- CA, EU, or US Cloud Web Client account

### 1. Create a Service Principal

- Log in to your account using Web Client as an administrator:

  - [CA Cloud](https://app.laserfiche.ca/laserfiche)
  - [EU Cloud](https://app.eu.laserfiche.com/laserfiche)
  - [US Cloud](https://app.laserfiche.com/laserfiche)

- Using the app picker, go to the 'Account' page.
- Click on the 'Service Principals' tab.
- Click on the 'Add Service Principal' button to create an account to be used to run this sample service.
- Add access rights to the repository and click the 'Create' button.
- View the created service principal and click on the 'Create Service Principal Key(s)' button.
- Save the Service Principal Key for later use.

### 2. Create an OAuth Service App

- Navigate to Laserfiche Developer Console:
  - [CA Cloud](https://app.laserfiche.ca/devconsole/)
  - [EU Cloud](https://app.eu.laserfiche.com/devconsole/)
  - [US Cloud](https://app.laserfiche.com/devconsole/)
- Click on the 'New' button and choose the 'Create a new app' option.
- Select the 'Service' option, enter a name, and click the 'Create application' button.
- Select the app service account to be the one created on step 1 and click the 'Save changes' button.
- Click on the 'Authentication' Tab and create a new Access Key.
- Select the first option (i.e. Create a public 'Access Key'...).
- Click the 'Download key as base-64 string' button for later use.
- Click OK.
- Select the required scope(s) in the 'Authentication' tab. For running this sample project, both 'repository.Read' and 'repository.Write' scopes are required.
- Click on the 'Update scopes' button.

### 3. Clone this repo on your local machine

### 4. Create a .env file

- Using the app picker, go to the 'Repository Administration' page and copy the Repository ID.
- In the root directory of this project, create a .env file containing the following lines:
```
AUTHORIZATION_TYPE="CLOUD_ACCESS_KEY" 

SERVICE_PRINCIPAL_KEY="<Service Principal Key created from step 1>"

ACCESS_KEY="<base-64 Access Key string created from step 2>"

REPOSITORY_ID="<Repository ID from the 'Repository Administration' page>"
```
- Note: The .env file is used in local development environment to set operating system environment variables.
  - DO NOT check-in the .env file in Git.

## Self-Hosted Prerequisites

See [Use v1 this page](https://github.com/Laserfiche/lf-sample-repository-api-dotnet-srv/tree/v1#self-hosted-prerequisites).

## Build and Run this App

- Open a terminal window.
- Enter the following commands:

```csharp
dotnet build
dotnet run
```

These commands will install, compile, and execute this program which will print out the repository information in the output window.
Note: This project uses the [Laserfiche.Repository.Api.Client.V2](https://www.nuget.org/packages/Laserfiche.Repository.Api.Client.V2) NuGet package. See [Laserfiche Repository API Documentation](https://developer.laserfiche.com/libraries.html).
