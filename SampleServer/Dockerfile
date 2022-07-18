FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src/MyServerBase
COPY MyServerBase .

RUN dotnet restore MyServerBase.csproj

WORKDIR /src/SampleServer
COPY SampleServer .
RUN dotnet restore SampleServer.csproj
RUN dotnet publish SampleServer.csproj -c Debug -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 3000

ENTRYPOINT ["./SampleServer"]
