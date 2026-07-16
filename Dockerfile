FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FinOrganizer.slnx ./
COPY Directory.Build.props ./
COPY src/FinOrganizer.Domain/FinOrganizer.Domain.csproj src/FinOrganizer.Domain/
COPY src/FinOrganizer.Application/FinOrganizer.Application.csproj src/FinOrganizer.Application/
COPY src/FinOrganizer.Infrastructure/FinOrganizer.Infrastructure.csproj src/FinOrganizer.Infrastructure/
COPY src/FinOrganizer.Api/FinOrganizer.Api.csproj src/FinOrganizer.Api/
RUN dotnet restore src/FinOrganizer.Api/FinOrganizer.Api.csproj

COPY src/ src/
RUN dotnet publish src/FinOrganizer.Api/FinOrganizer.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FinOrganizer.Api.dll"]
