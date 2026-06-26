FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.slnx ./

COPY Chat.Shared/*.csproj ./Chat.Shared/
COPY Chat.Server/*.csproj ./Chat.Server/
COPY Chat.Client/*.csproj ./Chat.Client/

RUN dotnet restore Chat.Server/Chat.Server.csproj

COPY . .

RUN dotnet publish Chat.Server/Chat.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Chat.Server.dll"]
