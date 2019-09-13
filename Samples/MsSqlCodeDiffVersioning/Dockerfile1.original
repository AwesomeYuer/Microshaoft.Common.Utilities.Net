FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.WebApplication.csproj", "Samples/MsSqlCodeDiffVersioning/"]
COPY ["StoreProcedureWebApiExecutor/StoreProcedureWebApiExecutor.Core/Microshaoft.StoreProcedureExecutorWebApiService.Core.csproj", "StoreProcedureWebApiExecutor/StoreProcedureWebApiExecutor.Core/"]
COPY ["StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/MsSQL.StoreProcedureWebApiExecutor.Plugin.csproj", "StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/"]
COPY ["StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/MySQL.StoreProcedureWebApiExecutor.Plugin.csproj", "StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/"]
RUN dotnet restore "Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.WebApplication.csproj"
RUN dotnet restore "StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/MsSQL.StoreProcedureWebApiExecutor.Plugin.csproj"
RUN dotnet restore "StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/MySQL.StoreProcedureWebApiExecutor.Plugin.csproj"
COPY . .
WORKDIR "/src/Samples/MsSqlCodeDiffVersioning"
RUN dotnet build "MsSqlCodeDiffVersioning.WebApplication.csproj" -c Release -o /app
RUN dotnet build "StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/MsSQL.StoreProcedureWebApiExecutor.Plugin.csproj" -c Release -o /app/CompositionPlugins
RUN dotnet build "StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/MySQL.StoreProcedureWebApiExecutor.Plugin.csproj" -c Release -o /app/CompositionPlugins

FROM build AS publish
RUN dotnet publish "MsSqlCodeDiffVersioning.WebApplication.csproj" -c Release -o /app
RUN dotnet publish "StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/MsSQL.StoreProcedureWebApiExecutor.Plugin.csproj" -c Release -o /app/CompositionPlugins
RUN dotnet publish "StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/MySQL.StoreProcedureWebApiExecutor.Plugin.csproj" -c Release -o /app/CompositionPlugins

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MsSqlCodeDiffVersioning.WebApplication.dll"]