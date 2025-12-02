FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5112

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier le csproj et restaurer
COPY ["AuthService.csproj", "./"]
RUN dotnet restore "AuthService.csproj"

# Copier TOUT le contenu (y compris Migrations/)
COPY . .

# Vérifier que les migrations sont bien là
RUN ls -la Migrations/ || echo "No Migrations folder found!"

# Build
RUN dotnet build "AuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Vérifier le contenu publié
RUN ls -la

ENV ASPNETCORE_URLS=http://0.0.0.0:5112
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AuthService.dll"]