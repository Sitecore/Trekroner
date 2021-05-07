# Need to use servercore for watch to work due to https://github.com/dotnet/aspnetcore/issues/22797
FROM mcr.microsoft.com/dotnet/sdk:5.0-windowsservercore-ltsc2019 as debug
WORKDIR /src
ENV DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER=true
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 80
# Required for HTTPS due to https://github.com/dotnet/dotnet-docker/issues/1048
USER ContainerAdministrator
ENTRYPOINT ["dotnet", "watch", "-v",  "run", "--project", ".\\Sitecore.Trekoner", "--no-launch-profile"]

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG VERSION

WORKDIR /app

# Copy csproj and restore as distinct layers
#COPY *.sln ./
#COPY Sitecore.Trekoner/*.csproj ./Sitecore.Trekoner
#COPY Sitecore.Trekroner.Client/*.csproj ./Sitecore.Trekroner.Client
#RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN echo %VERSION%
RUN dotnet publish /p:Version=%VERSION% .\Sitecore.Trekoner\Sitecore.Trekroner.csproj -c Release -o out

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