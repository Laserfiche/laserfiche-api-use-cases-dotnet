# Laserfiche Repository Client API Sample - Service App
Sample project for demonstrating how to work with our Repository API client.

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

(Currently, we have not yet released the 2.0 version (but will be soon), for now, we can use a local nuget package using command similar to the one below.)

```
dotnet add package Laserfiche.Repository.Api.Client --source {absolute_path_to_folder_containing_nupkg}
```

```
dotnet add package DotNetEnv
```

### Remove dependencies

```
dotnet remove package {package_id}
```
