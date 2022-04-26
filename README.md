# lf-sample-repository-api-dotnet-srv
Sample project for demonstrating how to work with our OAuth and Repository API clients.

## Steps to recreate this projecct

### Scafolding

```
dotnet new console --framework net6.0
```

### Run the project

```
dotnet run
```

### Add dependencies

```
dotnet add package Laserfiche.Oauth.Api.Client --version 1.0.0
```

```
dotnet add package Laserfiche.Repository.Api.Client --source {absolute_path_to_folder_containing_nupkg}
```

```
dotnet add package DotNetEnv
```