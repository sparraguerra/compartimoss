#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG NET_IMAGE=6.0-bullseye-slim
FROM mcr.microsoft.com/dotnet/aspnet:${NET_IMAGE} AS base 
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Demo.ApiGateway/Demo.ApiGateway.csproj", "Demo.ApiGateway/"]
RUN dotnet restore "Demo.ApiGateway/Demo.ApiGateway.csproj"
COPY . .
WORKDIR "/src/Demo.ApiGateway"
RUN dotnet build "Demo.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Demo.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Demo.ApiGateway.dll"]