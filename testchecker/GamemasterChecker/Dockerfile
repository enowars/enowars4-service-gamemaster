FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY GamemasterChecker.csproj .
RUN dotnet restore GamemasterChecker.csproj
COPY . .
RUN dotnet publish GamemasterChecker.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app .
COPY appsettings.json .
ENTRYPOINT ["dotnet", "Gamemaster.dll"]
