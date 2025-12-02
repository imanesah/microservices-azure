#FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
#WORKDIR /app
#EXPOSE 5157

#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#WORKDIR /src
#COPY . .
#RUN dotnet restore
#RUN dotnet publish -c Release -o /app/publish

#FROM base AS final
#WORKDIR /app
#COPY ocelot.json .
#COPY --from=build /app/publish .
#ENTRYPOINT ["dotnet", "ApiGateway.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5157

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ApiGateway.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:5157
ENTRYPOINT ["dotnet", "ApiGateway.dll"]