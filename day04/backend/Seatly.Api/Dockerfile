# Bước 1: build + publish (image SDK ~800MB, chỉ dùng lúc build)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY Seatly.Api.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# Bước 2: chỉ runtime (~120MB). Bản alpine có sẵn wget cho healthcheck.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Seatly.Api.dll"]
