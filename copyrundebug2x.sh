dotnet build CommonUtilities.NET.Core.Standard.sln -c Debug

mkdir -p Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/wwwroot/
cp -rf Samples/MsSqlCodeDiffVersioning/wwwroot/* Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/wwwroot/

mkdir -p Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MsSQL.Plugin/MsSQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Plugin*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Plugin*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp JTokenModelParameterValidatorsPlugins/SamplePlugin/SamplePlugin.2.x/bin/Debug/netcoreapp2.2/*Plugin* Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/

cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*MySql*   Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Npgsql*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Sqlite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*SQLite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Oracle*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*DB2*     Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/

cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*MySql*   Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Npgsql*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Sqlite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*SQLite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Oracle*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*DB2*     Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/

cd Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
dotnet MsSqlCodeDiffVersioning.WebApplication.dll $1
