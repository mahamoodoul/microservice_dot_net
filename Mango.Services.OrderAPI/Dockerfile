FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Mango.Services.OrderAPI/Mango.Services.OrderAPI.csproj", "Mango.Services.OrderAPI/"]
COPY ["Mango.MessageBus/Mango.MessageBus.csproj", "Mango.MessageBus/"]
RUN dotnet restore "Mango.Services.OrderAPI/Mango.Services.OrderAPI.csproj"
COPY . .
WORKDIR "/src/Mango.Services.OrderAPI"
RUN dotnet build "Mango.Services.OrderAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Mango.Services.OrderAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mango.Services.OrderAPI.dll"]
