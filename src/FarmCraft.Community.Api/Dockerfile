#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FarmCraft.Community.Api/FarmCraft.Community.Api.csproj", "FarmCraft.Community.Api/"]
RUN dotnet restore "FarmCraft.Community.Api/FarmCraft.Community.Api.csproj"
COPY . .
WORKDIR "/src/FarmCraft.Community.Api"
RUN dotnet build "FarmCraft.Community.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FarmCraft.Community.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FarmCraft.Community.Api.dll"]