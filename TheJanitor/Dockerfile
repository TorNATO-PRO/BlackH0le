FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TheJanitor/TheJanitor.fsproj", "TheJanitor/"]
RUN dotnet restore "TheJanitor/TheJanitor.fsproj"
COPY . .
WORKDIR "/src/TheJanitor"
RUN dotnet build "TheJanitor.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TheJanitor.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TheJanitor.dll"]
