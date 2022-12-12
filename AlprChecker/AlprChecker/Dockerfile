#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AlprChecker/AlprChecker.csproj", "AlprChecker/"]
RUN dotnet restore "AlprChecker/AlprChecker.csproj"
COPY . .
WORKDIR "/src/AlprChecker"
RUN dotnet build "AlprChecker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlprChecker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlprChecker.dll"]