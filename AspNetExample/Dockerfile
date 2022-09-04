FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AspNetExample/AspNetExample.csproj", "AspNetExample/"]
RUN dotnet restore "AspNetExample/AspNetExample.csproj"
COPY . .
WORKDIR "/src/AspNetExample"
RUN dotnet build "AspNetExample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AspNetExample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNetExample.dll"]
