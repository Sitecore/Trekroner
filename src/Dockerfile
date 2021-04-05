FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
#COPY *.sln ./
#COPY Sitecore.Trekoner/*.csproj ./Sitecore.Trekoner
#COPY Sitecore.Trekroner.Client/*.csproj ./Sitecore.Trekroner.Client
#RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish .\Sitecore.Trekoner\Sitecore.Trekroner.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0

# Certificate tools for the host
WORKDIR /
COPY ./tools .

# Our build output
WORKDIR /app
COPY --from=build /app/out .

# Required for HTTPS due to https://github.com/dotnet/dotnet-docker/issues/1048
USER ContainerAdministrator

ENTRYPOINT ["dotnet", "Sitecore.Trekroner.dll"]