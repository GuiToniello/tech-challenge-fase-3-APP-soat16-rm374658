# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia primeiro os csproj para maximizar o cache do restore.
COPY ["1 - Frameworks & Drivers/TechChallenge.Oficina.CreateOSService.API/TechChallenge.Oficina.CreateOSService.API.csproj", "1 - Frameworks & Drivers/TechChallenge.Oficina.CreateOSService.API/"]
COPY ["1 - Frameworks & Drivers/TechChallenge.Oficina.DB/TechChallenge.Oficina.DB.csproj", "1 - Frameworks & Drivers/TechChallenge.Oficina.DB/"]
COPY ["1 - Frameworks & Drivers/TechChallenge.Oficina.Email/TechChallenge.Oficina.Email.csproj", "1 - Frameworks & Drivers/TechChallenge.Oficina.Email/"]
COPY ["2 - Interface Adapters/TechChallenge.Oficina.Adapters/TechChallenge.Oficina.Adapters.csproj", "2 - Interface Adapters/TechChallenge.Oficina.Adapters/"]
COPY ["2 - Interface Adapters/TechChallenge.Oficina.Controllers/TechChallenge.Oficina.Controllers.csproj", "2 - Interface Adapters/TechChallenge.Oficina.Controllers/"]
COPY ["3 - Application Business Rules/TechChallenge.Oficina.UseCases/TechChallenge.Oficina.UseCases.csproj", "3 - Application Business Rules/TechChallenge.Oficina.UseCases/"]
COPY ["4 - Enterprise Business Rules/TechChallenge.Oficina.Entities/TechChallenge.Oficina.Entities.csproj", "4 - Enterprise Business Rules/TechChallenge.Oficina.Entities/"]

RUN dotnet restore "1 - Frameworks & Drivers/TechChallenge.Oficina.CreateOSService.API/TechChallenge.Oficina.CreateOSService.API.csproj"

COPY ["1 - Frameworks & Drivers/TechChallenge.Oficina.CreateOSService.API/", "1 - Frameworks & Drivers/TechChallenge.Oficina.CreateOSService.API/"]
COPY ["1 - Frameworks & Drivers/TechChallenge.Oficina.DB/", "1 - Frameworks & Drivers/TechChallenge.Oficina.DB/"]
COPY ["1 - Frameworks & Drivers/TechChallenge.Oficina.Email/", "1 - Frameworks & Drivers/TechChallenge.Oficina.Email/"]
COPY ["2 - Interface Adapters/TechChallenge.Oficina.Adapters/", "2 - Interface Adapters/TechChallenge.Oficina.Adapters/"]
COPY ["2 - Interface Adapters/TechChallenge.Oficina.Controllers/", "2 - Interface Adapters/TechChallenge.Oficina.Controllers/"]
COPY ["3 - Application Business Rules/TechChallenge.Oficina.UseCases/", "3 - Application Business Rules/TechChallenge.Oficina.UseCases/"]
COPY ["4 - Enterprise Business Rules/TechChallenge.Oficina.Entities/", "4 - Enterprise Business Rules/TechChallenge.Oficina.Entities/"]
WORKDIR "/src/1 - Frameworks & Drivers/TechChallenge.Oficina.CreateOSService.API"
RUN dotnet publish "TechChallenge.Oficina.CreateOSService.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

USER $APP_UID
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=20s --retries=3 \
	CMD curl --fail --silent --show-error http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "TechChallenge.Oficina.CreateOSService.API.dll"]
