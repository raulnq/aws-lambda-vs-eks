dotnet restore MinimalAPI

dotnet lambda package -pl MinimalAPI --configuration Release --framework net6.0 --output-package MinimalAPI/bin/Release/net6.0/MinimalAPI.zip

dotnet restore Lambda

dotnet lambda package -pl Lambda --configuration Release --framework net6.0 --output-package Lambda/bin/Release/net6.0/Lambda.zip