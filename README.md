# laserfiche-api-use-cases-dotnet

Sample .NET C# console application [code](./Program.cs) with sample use cases for:
- Creating and using a Laserfiche Repository API client to interact with Laserfiche repositories.
- Creating and using a Laserfiche OData API client to interact with a Laserfiche lookup tables.

## Prerequisites

NOTE: The version used by this branch is NOT compatible with Laserfiche self-hosted repositories. Use [this branch](https://github.com/Laserfiche/laserfiche-api-use-cases-dotnet/tree/v1#self-hosted-prerequisites) for sample code compatible self-hosted repositories.

- A Laserfiche Cloud Account
- .NET 6.0 core SDK

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
- Select the required scope(s) in the 'Authentication' tab. For running this sample project these scopes are required:
  - For repository API sample code: `repository.Read repository.Write`
  - For OData API sample code: `table.Read table Write project/Global`
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

### 5. Create a test Lookup Table

 - Using Web Client, navigate to Process Automation / Data Management (Global)
 - Create a new Lookup table named `ALL_DATA_TYPES_TABLE_SAMPLE` by uploading file [TestFiles/ALL_DATA_TYPES_TABLE_SAMPLE.csv](./TestFiles/ALL_DATA_TYPES_TABLE_SAMPLE.csv)

## Build and Run this App

- Open a terminal window.
- Enter the following commands:

```csharp
dotnet build
dotnet run
```

These commands will install, compile, and execute this program which will print out various use cases defined in
[Program.cs](./Program.cs). This contains samples for both the repository and table APIs, but they can be run independently as needed.
