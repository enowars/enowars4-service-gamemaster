FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

# Fetch dependencies
COPY GamemasterChecker.sln .
COPY GamemasterChecker/GamemasterChecker.csproj GamemasterChecker/GamemasterChecker.csproj
RUN dotnet restore

# Publish
COPY . .
RUN dotnet publish -c Release --self-contained true -o /build

# Copy to runtime container
FROM ghcr.io/enowars/enochecker-dotnet:nightly
COPY --from=build /build /GamemasterChecker

CMD ["/GamemasterChecker/GamemasterChecker.dll"]
