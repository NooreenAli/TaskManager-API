FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY TaskManager.sln .
COPY src/TaskManager.Worker/TaskManager.Worker.csproj src/TaskManager.Worker/
COPY src/TaskManager.Core/TaskManager.Core.csproj src/TaskManager.Core/
COPY src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj src/TaskManager.Infrastructure/
RUN dotnet restore src/TaskManager.Worker/TaskManager.Worker.csproj

COPY . .
RUN dotnet publish src/TaskManager.Worker/TaskManager.Worker.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TaskManager.Worker.dll"]