FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TodoAPI.csproj ./
RUN dotnet restore TodoAPI.csproj
COPY . ./
RUN dotnet publish TodoAPI.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=10000
USER $APP_UID
EXPOSE 10000
ENTRYPOINT ["dotnet", "TodoAPI.dll"]
