dotnet build CommonUtilities.NET.Core.Standard.sln -c Debug

mkdir -p Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/wwwroot/
cp -rf Samples/MsSqlCodeDiffVersioning/wwwroot/* Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/wwwroot/

mkdir -p Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MsSQL.Plugin/MsSQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Plugin*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Plugin*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp JTokenModelParameterValidatorsPlugins/SamplePlugin/SamplePlugin.2.x/bin/Debug/netcoreapp2.2/*Plugin* Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/CompositionPlugins/

cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*MySql*   Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Npgsql*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Sqlite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*SQLite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Oracle*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*DB2*     Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/CompositionPlugins/

cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*MySql*   Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Npgsql*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Sqlite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*SQLite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*Oracle*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.2.x/bin/Debug/netcoreapp2.2/*DB2*     Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/

cd Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.2x/bin/Debug/netcoreapp2.2/
dotnet MsSqlCodeDiffVersioning.2x.dll $1
