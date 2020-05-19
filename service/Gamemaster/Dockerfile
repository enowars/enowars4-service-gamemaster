FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY Gamemaster.csproj .
RUN dotnet restore Gamemaster.csproj
COPY [^Frontend]* ./
RUN dotnet publish Gamemaster.csproj -c Release -o /app

FROM npm as frontend


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Gamemaster.dll"]
