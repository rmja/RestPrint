# RestPrint

RestPrint is a simple rest api that exposes the printers on the machine on where it is run through a simple JSON based REST api.

Examples actions are:

Get all installed printers:
```
curl -k https://localhost:9721/Printers
```

Print raw (ESC/POS, Zebra, etc.):
```
curl -k https://localhost:9721/Printers/MyPrinter/Jobs -X POST -H "Content-Type: application/octet-stream" --data-binary @myfile.bin
```

## Test Tool
dotnet pack
dotnet tool install -g --add-source ./bin/Debug restprint
dotnet tool uninstall -g restprint

# Deploy Tool
dotnet pack --configuration Release
dotnet nuget push .\bin\Release\RestPrint.0.0.1.nupkg --source https://api.nuget.org/v3/index.json --api-key <API-KEY>