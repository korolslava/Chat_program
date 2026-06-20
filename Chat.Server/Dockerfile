FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln ./

COPY Chat.Shared/*.csproj ./Chat.Shared/
COPY Chat.Server/*.csproj ./Chat.Server/

RUN ls -R

RUN dotnet restore

COPY . .

RUN dotnet publish Chat.Server/Chat.Server.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Chat.Server.dll"]