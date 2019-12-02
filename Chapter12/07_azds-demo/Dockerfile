FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["azds_demo.csproj", "./"]

RUN dotnet restore "./azds_demo.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "azds_demo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "azds_demo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "azds_demo.dll"]