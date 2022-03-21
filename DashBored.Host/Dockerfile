#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DashBored.Host/DashBored.Host.csproj", "DashBored.Host/"]
COPY ["Plugin.RTSP/Plugin.RTSP.csproj", "Plugin.RTSP/"]
COPY ["DashBored.PluginApi/DashBored.PluginApi.csproj", "DashBored.PluginApi/"]
COPY ["Plugin.Clock/Plugin.Clock.csproj", "Plugin.Clock/"]
RUN dotnet restore "DashBored.Host/DashBored.Host.csproj"
COPY . .
WORKDIR "/src/DashBored.Host"
RUN dotnet build "DashBored.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DashBored.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DashBored.Host.dll"]