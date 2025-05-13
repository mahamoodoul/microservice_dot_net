FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Mango.Services.EmailAPI/Mango.Services.EmailAPI.csproj", "Mango.Services.EmailAPI/"]
RUN dotnet restore "Mango.Services.EmailAPI/Mango.Services.EmailAPI.csproj"
COPY . .
WORKDIR "/src/Mango.Services.EmailAPI"
RUN dotnet build "Mango.Services.EmailAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Mango.Services.EmailAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mango.Services.EmailAPI.dll"]
