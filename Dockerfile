# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["PopulationCouncil.sln", "./"]
COPY ["Api/Api.csproj", "Api/"]
COPY ["BackgroundService/BackgroundServices.csproj", "BackgroundService/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Service/Service.csproj", "Service/"]

RUN dotnet restore

# Copy all source files and compile
COPY . .
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Api.dll"]
