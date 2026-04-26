FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY TaskManager.sln .
COPY src/TaskManager.API/TaskManager.API.csproj src/TaskManager.API/
COPY src/TaskManager.Core/TaskManager.Core.csproj src/TaskManager.Core/
COPY src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj src/TaskManager.Infrastructure/
RUN dotnet restore src/TaskManager.API/TaskManager.API.csproj

COPY . .
RUN dotnet publish src/TaskManager.API/TaskManager.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManager.API.dll"]