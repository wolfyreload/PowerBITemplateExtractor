rmdir /S /Q bin
dotnet clean
dotnet restore
dotnet build PowerBITemplateExtractor\PowerBITemplateExtractor.csproj --configuration release --output ..\bin