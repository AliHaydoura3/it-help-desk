FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/ ./backend/
RUN dotnet restore backend/HelpDesk.Api/HelpDesk.Api.csproj
RUN dotnet publish backend/HelpDesk.Api/HelpDesk.Api.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN apt-get update && apt-get install --yes --no-install-recommends fonts-dejavu-core && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "HelpDesk.Api.dll"]
