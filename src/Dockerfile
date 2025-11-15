FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ConsoleApp/ConsoleApp.csproj", "ConsoleApp/"]
COPY ["SmoLibrary/SmoLibrary.csproj", "SmoLibrary/"]
RUN dotnet restore "ConsoleApp/ConsoleApp.csproj"
COPY . .
WORKDIR "/src/ConsoleApp"
RUN dotnet build "./ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV svm_targets=0-1-2-3-4-5-6-7-8
ENTRYPOINT ["sh", "-c", "dotnet ConsoleApp.dll"]
