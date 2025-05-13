# -----------------------------------------------------
# 1) BUILD IMAGE (SDK)
# -----------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy solution and project files
COPY Mango.sln .
COPY ["Mango.Services.AuthAPI/Mango.Services.AuthAPI.csproj", "Mango.Services.AuthAPI/"]
COPY ["Mango.MessageBus/Mango.MessageBus.csproj", "Mango.MessageBus/"]

# Restore dependencies
RUN dotnet restore "Mango.Services.AuthAPI/Mango.Services.AuthAPI.csproj"

# Copy remaining source code
COPY . .

# Build project
WORKDIR "Mango.Services.AuthAPI"
RUN dotnet build "Mango.Services.AuthAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# -----------------------------------------------------
# 2) PUBLISH IMAGE
# -----------------------------------------------------
FROM build AS publish

ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Mango.Services.AuthAPI.csproj" -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# -----------------------------------------------------
# 3) RUNTIME IMAGE (ASP.NET Core)
# -----------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# (Optional) Run as non-root user if the base image supports $APP_UID
# Make sure $APP_UID is set in the base image or your environment
USER $APP_UID
WORKDIR /app

# Network ports (make sure your app actually uses these ports)
EXPOSE 8080

# Force ASP.NET to listen on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Mango.Services.AuthAPI.dll"]