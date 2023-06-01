#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:4 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DaprAndAzureFunctions.csproj", "."]
RUN dotnet restore "./DaprAndAzureFunctions.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "DaprAndAzureFunctions.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DaprAndAzureFunctions.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true