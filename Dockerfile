FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FlowOps.Api/FlowOps.Api.csproj", "FlowOps.Api/"]
COPY ["FlowOps.Application/FlowOps.Application.csproj", "FlowOps.Application/"]
COPY ["FlowOps.Domain/FlowOps.Domain.csproj", "FlowOps.Domain/"]
COPY ["FlowOps.Infrastructure/FlowOps.Infrastructure.csproj", "FlowOps.Infrastructure/"]
RUN dotnet restore "FlowOps.Api/FlowOps.Api.csproj"
COPY . .
WORKDIR "/src/FlowOps.Api"
RUN dotnet build "FlowOps.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowOps.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowOps.Api.dll"]
