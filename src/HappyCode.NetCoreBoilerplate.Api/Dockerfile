FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["src/HappyCode.NetCoreBoilerplate.Api/HappyCode.NetCoreBoilerplate.Api.csproj", "src/HappyCode.NetCoreBoilerplate.Api/"]
COPY ["src/HappyCode.NetCoreBoilerplate.Core/HappyCode.NetCoreBoilerplate.Core.csproj", "src/HappyCode.NetCoreBoilerplate.Core/"]
RUN dotnet restore "src/HappyCode.NetCoreBoilerplate.Api/HappyCode.NetCoreBoilerplate.Api.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "src/HappyCode.NetCoreBoilerplate.Api/HappyCode.NetCoreBoilerplate.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/HappyCode.NetCoreBoilerplate.Api/HappyCode.NetCoreBoilerplate.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HappyCode.NetCoreBoilerplate.Api.dll"] 