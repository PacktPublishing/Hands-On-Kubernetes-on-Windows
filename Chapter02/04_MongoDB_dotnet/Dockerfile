FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

COPY *.csproj .
RUN dotnet restore

COPY *.cs .
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime
WORKDIR /app

COPY --from=build /app/out ./
ENTRYPOINT ["04_MongoDB_dotnet.exe"]