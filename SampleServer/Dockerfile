FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY SampleServer.csproj SampleServer.csproj
RUN dotnet restore SampleServer.csproj

COPY . .
RUN dotnet publish SampleServer.csproj -c Debug -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./SampleServer"]