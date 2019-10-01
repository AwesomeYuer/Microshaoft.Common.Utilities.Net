dotnet build CommonUtilities.NET.Core.Standard.3.0.sln -c Debug

mkdir -p Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/wwwroot/
cp -rf Samples/MsSqlCodeDiffVersioning/wwwroot/* Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/wwwroot/

mkdir -p Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MsSQL.Plugin/MsSQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Plugin*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Plugin*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp JTokenModelParameterValidatorsPlugins/SamplePlugin/SamplePlugin.3.x/bin/Debug/netcoreapp3.0/*Plugin* Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/CompositionPlugins/

cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*MySql*   Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Npgsql*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Sqlite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*SQLite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Oracle*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*DB2*     Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/CompositionPlugins/

cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*MySql*   Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Npgsql*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Sqlite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*SQLite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Oracle*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*DB2*     Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/

cd Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp3.0/
dotnet MsSqlCodeDiffVersioning.WebApplication.dll $1
