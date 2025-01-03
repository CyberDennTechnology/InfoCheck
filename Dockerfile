# Use the official .NET runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InfoCheckWebApplication/InfoCheckWebApplication.csproj", "./"]
RUN dotnet restore "InfoCheckWebApplication.csproj"
COPY . .
RUN dotnet publish "InfoCheckWebApplication.csproj" -c Release -o /app/publish

# Final stage: runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InfoCheckWebApplication.dll"]

