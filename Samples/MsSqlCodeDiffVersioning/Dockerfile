FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 5001
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.WebApplication.csproj", "Samples/MsSqlCodeDiffVersioning/"]
COPY ["StoreProcedureWebApiExecutor/StoreProcedureWebApiExecutor.Core/Microshaoft.StoreProcedureExecutorWebApiService.Core.csproj", "StoreProcedureWebApiExecutor/StoreProcedureWebApiExecutor.Core/"]
COPY ["JTokenModelParameterValidatorsPlugins/JTokenModelParameterValidatorSamplePlugin/JTokenModelParameterValidatorSamplePlugin.csproj", "JTokenModelParameterValidatorsPlugins/JTokenModelParameterValidatorSamplePlugin/"]
COPY ["StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/MsSQL.StoreProcedureWebApiExecutor.Plugin.csproj", "StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/"]
COPY ["StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/MySQL.StoreProcedureWebApiExecutor.Plugin.csproj", "StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/"]
RUN dotnet restore "JTokenModelParameterValidatorsPlugins/JTokenModelParameterValidatorSamplePlugin/JTokenModelParameterValidatorSamplePlugin.csproj"
RUN dotnet restore "StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/MsSQL.StoreProcedureWebApiExecutor.Plugin.csproj"
RUN dotnet restore "StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/MySQL.StoreProcedureWebApiExecutor.Plugin.csproj"
RUN dotnet restore "Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.WebApplication.csproj"

COPY . .
RUN dotnet build "JTokenModelParameterValidatorsPlugins/JTokenModelParameterValidatorSamplePlugin/JTokenModelParameterValidatorSamplePlugin.csproj" -c Release -o /app/CompositionPlugins
RUN dotnet build "StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/MsSQL.StoreProcedureWebApiExecutor.Plugin.csproj" -c Release -o /app/CompositionPlugins
RUN dotnet build "StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/MySQL.StoreProcedureWebApiExecutor.Plugin.csproj" -c Release -o /app/CompositionPlugins
RUN dotnet build "Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.WebApplication.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "JTokenModelParameterValidatorsPlugins/JTokenModelParameterValidatorSamplePlugin/JTokenModelParameterValidatorSamplePlugin.csproj" -c Release -o /app/CompositionPlugins
RUN dotnet publish "StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/MsSQL.StoreProcedureWebApiExecutor.Plugin.csproj" -c Release -o /app/CompositionPlugins
RUN dotnet publish "StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/MySQL.StoreProcedureWebApiExecutor.Plugin.csproj" -c Release -o /app/CompositionPlugins
RUN dotnet publish "Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.WebApplication.csproj" -c Release -o /app
COPY ["StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/MySQL.StoreProcedureWebApiExecutor.Plugin.csproj", "StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/"]