#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FarmCraft.Community.Core/FarmCraft.Community.Core.csproj", "FarmCraft.Community.Core/"]
RUN dotnet restore "FarmCraft.Community.Core/FarmCraft.Community.Core.csproj"
COPY . .
WORKDIR "/src/FarmCraft.Community.Core"
RUN dotnet build "FarmCraft.Community.Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FarmCraft.Community.Core.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FarmCraft.Community.Core.dll"]