#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG NET_IMAGE=6.0-bullseye-slim
FROM mcr.microsoft.com/dotnet/aspnet:${NET_IMAGE} AS base 
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Demo.Customers.Api/Demo.Customers.Api.csproj", "Demo.Customers.Api/"]
RUN dotnet restore "Demo.Customers.Api/Demo.Customers.Api.csproj"
COPY . .
WORKDIR "/src/Demo.Customers.Api"
RUN dotnet build "Demo.Customers.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Demo.Customers.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Demo.Customers.Api.dll"]