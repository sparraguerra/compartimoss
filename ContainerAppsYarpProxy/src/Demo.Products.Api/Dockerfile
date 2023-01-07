#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG NET_IMAGE=6.0-bullseye-slim
FROM mcr.microsoft.com/dotnet/aspnet:${NET_IMAGE} AS base 
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Demo.Products.Api/Demo.Products.Api.csproj", "Demo.Products.Api/"]
RUN dotnet restore "Demo.Products.Api/Demo.Products.Api.csproj"
COPY . .
WORKDIR "/src/Demo.Products.Api"
RUN dotnet build "Demo.Products.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Demo.Products.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Demo.Products.Api.dll"]