# Usare un'immagine base di .NET per buildare l'app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Usare un'immagine base di .NET per costruire l'app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["distributedDeliveryBackend/distributedDeliveryBackend.csproj", "distributedDeliveryBackend/"]
COPY ["Grains/Grains.csproj", "Grains/"]
COPY ["Abstractions/Abstractions.csproj", "Abstractions/"]
RUN dotnet restore "distributedDeliveryBackend/distributedDeliveryBackend.csproj"
COPY . .
WORKDIR "/src/distributedDeliveryBackend"
RUN dotnet build "distributedDeliveryBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "distributedDeliveryBackend.csproj" -c Release -o /app/publish

# Preparare l'immagine finale per il runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "distributedDeliveryBackend.dll"]
