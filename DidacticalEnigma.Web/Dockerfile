FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["DidacticalEnigma.Web/DidacticalEnigma.Web.csproj", "DidacticalEnigma.Web/"]
RUN dotnet restore "DidacticalEnigma.Web/DidacticalEnigma.Web.csproj"
COPY . .
WORKDIR "/src/DidacticalEnigma.Web"
RUN dotnet build "DidacticalEnigma.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DidacticalEnigma.Web.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DidacticalEnigma.Web.dll"]