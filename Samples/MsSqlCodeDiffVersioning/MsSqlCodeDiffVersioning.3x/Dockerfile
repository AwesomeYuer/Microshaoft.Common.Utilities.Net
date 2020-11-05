#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
COPY ["Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.Shared/wwwroot/", "/usr/local/wwwroot/"]
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/MsSqlCodeDiffVersioning.3x.csproj", "Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/"]
COPY ["Microshaoft.WebUtils/Microshaoft.WebUtils.NET.Core.3.x/Microshaoft.WebUtils.NET.Core.3.x.csproj", "Microshaoft.WebUtils/Microshaoft.WebUtils.NET.Core.3.x/"]
COPY ["Microshaoft.SqlExecutors/Microshaoft.SqlExecutors.NET.Std.2.x/Microshaoft.SqlExecutors.NET.Std.2.x.csproj", "Microshaoft.SqlExecutors/Microshaoft.SqlExecutors.NET.Std.2.x/"]
COPY ["Microshaoft.Json/Microshaoft.Json.NET.Std.2.x/Microshaoft.Json.NET.Std.2.x.csproj", "Microshaoft.Json/Microshaoft.Json.NET.Std.2.x/"]
COPY ["CommonUtils/CommonUtils.Lite.Std.2.x/CommonUtils.Lite.Std.2.x.csproj", "CommonUtils/CommonUtils.Lite.Std.2.x/"]
COPY ["Plugins/JsonReqValidators/SamplePlugin/SamplePlugin.NET.Core.3.x/JsonReqValidatorSamplePlugin.NET.Core.3.x.csproj", "Plugins/JsonReqValidators/SamplePlugin/SamplePlugin.NET.Core.3.x/"]
COPY ["Plugins/SqlExecutors/MsSQL/MsSQL.Plugin.NET.Std.2.x/MsSQL.SqlExecutor.Plugin.NET.Std.2.x.csproj", "Plugins/SqlExecutors/MsSQL/MsSQL.Plugin.NET.Std.2.x/"]
COPY ["Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/MySQL.SqlExecutor.Plugin.NET.Core.3.x.csproj", "Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/"]
RUN dotnet restore "Plugins/JsonReqValidators/SamplePlugin/SamplePlugin.NET.Core.3.x/JsonReqValidatorSamplePlugin.NET.Core.3.x.csproj"
RUN dotnet restore "Plugins/SqlExecutors/MsSQL/MsSQL.Plugin.NET.Std.2.x/MsSQL.SqlExecutor.Plugin.NET.Std.2.x.csproj"
RUN dotnet restore "Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/MySQL.SqlExecutor.Plugin.NET.Core.3.x.csproj"
RUN dotnet restore "Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/MsSqlCodeDiffVersioning.3x.csproj"
COPY . .
WORKDIR "/src/Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x"
RUN dotnet build "MsSqlCodeDiffVersioning.3x.csproj" -c Release -o /app/build
RUN dotnet build "../../../Plugins/JsonReqValidators/SamplePlugin/SamplePlugin.NET.Core.3.x/JsonReqValidatorSamplePlugin.NET.Core.3.x.csproj" -c Release -o /app/build/Plugins
RUN dotnet build "../../../Plugins/SqlExecutors/MsSQL/MsSQL.Plugin.NET.Std.2.x/MsSQL.SqlExecutor.Plugin.NET.Std.2.x.csproj" -c Release -o /app/build/Plugins
RUN dotnet build "../../../Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/MySQL.SqlExecutor.Plugin.NET.Core.3.x.csproj" -c Release -o /app/build/Plugins

#FROM build AS publish
#COPY ["/app/build/Plugins/SqlExecutors/MsSQL/MsSQL.Plugin.NET.Std.2.x/bin/Release/netstandard2.1/Microshaoft*.dll"				, "/app/build/Plugins/"]
#COPY ["/app/build/Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Release/netcoreapp3.1/Microshaoft*.dll"				, "/app/build/Plugins/"]
#COPY ["/app/build/Plugins/JsonReqValidators/SamplePlugin/SamplePlugin.NET.Core.3.x/bin/Release/netcoreapp3.1/Microshaoft*.dll"	, "/app/build/Plugins/"]
#COPY ["/app/build/Plugins/SqlExecutors/MsSQL/MsSQL.Plugin.NET.Std.2.x/bin/Release/netstandard2.1/Microshaoft*.pdb"				, "/app/build/Plugins/"]
#COPY ["/app/build/Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Release/netcoreapp3.1/Microshaoft*.pdb"				, "/app/build/Plugins/"]
#COPY ["/app/build/Plugins/JsonReqValidators/SamplePlugin/SamplePlugin.NET.Core.3.x/bin/Release/netcoreapp3.1/Microshaoft*.pdb"	, "/app/build/Plugins/"]
#COPY ["/app/build/Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Release/netcoreapp3.1/*MySql.Data*"					, "/app/build/Plugins/"]
#COPY ["/app/build/Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Release/netcoreapp3.1/*Npgsql*"						, "/app/build/Plugins/"]
#COPY ["/app/build/Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Release/netcoreapp3.1/*SQLite*"						, "/app/build/Plugins/"]
#COPY ["/app/build/Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Release/netcoreapp3.1/*Oracle*"						, "/app/build/Plugins/"]
## COPY ["/app/build/Plugins/SqlExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Release/netcoreapp3.1/*db2*"							, "/app/build/Plugins/"]
#RUN dotnet publish "MsSqlCodeDiffVersioning.3x.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "MsSqlCodeDiffVersioning.3x.dll"]