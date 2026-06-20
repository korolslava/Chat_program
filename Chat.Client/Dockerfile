FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.sln* ./

COPY Chat.Shared/*.csproj ./Chat.Shared/
COPY Chat.Client/*.csproj ./Chat.Client/

RUN dotnet restore

COPY . .

RUN dotnet publish Chat.Client/Chat.Client.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Chat.Client.dll"]
