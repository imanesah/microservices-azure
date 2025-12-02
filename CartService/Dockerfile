FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5236

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier le csproj et restaurer
COPY ["CartService.csproj", "./"]
RUN dotnet restore "CartService.csproj"

# Copier tout le contenu
COPY . .

# Build
RUN dotnet build "CartService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CartService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:5236
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CartService.dll"]