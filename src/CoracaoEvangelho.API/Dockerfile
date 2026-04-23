FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/CoracaoEvangelho.API/CoracaoEvangelho.API.csproj ./CoracaoEvangelho.API/
RUN dotnet restore ./CoracaoEvangelho.API/CoracaoEvangelho.API.csproj

COPY . .
WORKDIR /src/CoracaoEvangelho.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:$PORT

ENTRYPOINT ["dotnet", "CoracaoEvangelho.API.dll"]