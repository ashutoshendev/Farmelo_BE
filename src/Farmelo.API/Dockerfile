FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Farmelo.API/Farmelo.API.csproj", "src/Farmelo.API/"]
COPY ["src/Farmelo.Business/Farmelo.Business.csproj", "src/Farmelo.Business/"]
COPY ["src/Farmelo.Data/Farmelo.Data.csproj", "src/Farmelo.Data/"]
COPY ["src/Farmelo.Shared/Farmelo.Shared.csproj", "src/Farmelo.Shared/"]
COPY ["src/Farmelo.AutoMapper/Farmelo.AutoMapper.csproj", "src/Farmelo.AutoMapper/"]
COPY ["src/Farmelo.Integration/Farmelo.Integration.csproj", "src/Farmelo.Integration/"]
RUN dotnet restore "src/Farmelo.API/Farmelo.API.csproj"
COPY . .
WORKDIR "/src/src/Farmelo.API"
RUN dotnet publish "Farmelo.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Farmelo.API.dll"]
