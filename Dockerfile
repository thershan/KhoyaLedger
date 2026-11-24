# Build stage using official Microsoft .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["KhoyaLedger.csproj", "./"]
RUN dotnet restore "KhoyaLedger.csproj"

# Copy remaining source code and publish in Release mode
COPY . .
RUN dotnet publish "KhoyaLedger.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage using lightweight ASP.NET Core 10 runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8080 for Render / Cloud hosting
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "KhoyaLedger.dll"]
