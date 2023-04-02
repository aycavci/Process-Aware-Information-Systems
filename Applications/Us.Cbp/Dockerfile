FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY Us.Cbp.csproj ./
RUN dotnet restore "./Us.Cbp.csproj" --disable-parallel
COPY . .
WORKDIR "/src/."
RUN dotnet build "Us.Cbp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Us.Cbp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Us.Cbp.dll"]