# Use .NET SDK for building the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /source

# Copy solution file and project files
COPY WanviBE.sln .
COPY ./Wanvi.Contract.Repositories ./Wanvi.Contract.Repositories
COPY ./Wanvi.Core ./Wanvi.Core
COPY ./Wanvi.ModelViews ./Wanvi.ModelViews
COPY ./Wanvi.Repositories ./Wanvi.Repositories
COPY ./Wanvi.Contract.Services ./Wanvi.Contract.Services
COPY ./Wanvi.Services ./Wanvi.Services
COPY ./Wanvi.API ./Wanvi.API

# Restore dependencies
RUN dotnet restore

# Build the application
WORKDIR /source/Wanvi.API
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish ./Wanvi.API.csproj --use-current-runtime --self-contained false -o /app

# Use minimal runtime for final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Enable globalization and time zones
RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copy published app
COPY --from=build /app .

# Switch to non-root user
USER app

ENTRYPOINT ["dotnet", "Wanvi.API.dll"]
