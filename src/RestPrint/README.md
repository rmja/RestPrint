
## Test Tool
dotnet pack
dotnet tool install -g --add-source ./bin/Debug restprint
dotnet tool uninstall -g restprint

# Deploy Tool
dotnet pack --configuration Release
dotnet nuget push .\bin\Release\RestPrint.0.0.1.nupkg --source https://api.nuget.org/v3/index.json --api-key <API-KEY>