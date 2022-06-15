# lf-sample-repository-api-dotnet-srv

Sample .NET core service app that connects to a Laserfiche Cloud Repository using a service principal account.
[Sample Code](./Program.cs)

### Prerequisites

#### Software Prerequisites

- Visual Studio Code
- .NET 6.0 core SDK
- Git
- Clone this repo on your local machine

#### 1. Create a Service Principal

- Login to account using web client as an administrator
- Using the app picker, go to the account page
- Click on the service principal tab
- Click on the 'Add Service Principal' button to create an account to be used to run this sample service
- Add access rights to the repository and click create
- View the created service principal and click on 'Create Service Principal Key(s)'
- Save the key for later use

#### 2. Create an OAuth Service App

- Navigate to [Laserfiche Developer Console](https://app.laserfiche.com/devconsole/)
- Click on 'New' -> 'Create a new app'
- Select 'Service', enter a name
- Select the app service account to be the one created on step 1 and click save
- Click on the 'Authentication' Tab and create a new AccessKey
- Click the 'Download key as base-64 string' button for later use

#### 3. Create a .env file

- Using the app picker, go to the 'Repository Administration' page and copy the Repository ID
- In the root directory of this project, create an .env file containing the following lines:

```bash
SERVICE_PRINCIPAL_KEY="<Service Principal Key created from step 1>"

ACCESS_KEY="<base-64 AccessKey string created from step 2>"

REPOSITORY_ID="<Repository ID from the 'Repository Administration' page>"
```

- Note: The .env file is used in local development environment to set operating system environment variables. DO NOT check-in the .env file in Git 

## Build and Run this App

- Open the sample project root folder in Visual Studio Code
- On a terminal window, enter the following commands:

```bash
dotnet build
dotnet run
```

These commands will install, compile, and execute this program which will print out the repository information in the output window.
Note: This project uses the [Laserfiche.Repository.Api.Client NuGet package](https://www.nuget.org/packages/Laserfiche.Repository.Api.Client). See [Laserfiche Repository API Documentation](https://developer.laserfiche.com/libraries.html).
