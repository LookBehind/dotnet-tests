FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SocketExample/SocketExample.csproj", "SocketExample/"]
RUN dotnet restore "SocketExample/SocketExample.csproj"
COPY . .
WORKDIR "/src/SocketExample"
RUN dotnet build "SocketExample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SocketExample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SocketExample.dll"]
