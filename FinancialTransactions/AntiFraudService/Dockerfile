FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AntiFraudService/AntiFraudService.csproj", "AntiFraudService/"]
RUN dotnet restore "AntiFraudService/AntiFraudService.csproj"
COPY . .
WORKDIR "/src/AntiFraudService"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AntiFraudService.dll"]