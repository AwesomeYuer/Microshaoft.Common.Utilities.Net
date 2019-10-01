dotnet build CommonUtilities.NET.Core.Standard.3.0.sln -c Debug

mkdir -p Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/wwwroot/
cp -rf Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/wwwroot/* Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/wwwroot/

mkdir -p Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MsSQL.Plugin/MsSQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Plugin*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Plugin*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp JTokenModelParameterValidatorsPlugins/SamplePlugin/SamplePlugin.3.x/bin/Debug/netcoreapp3.0/*Plugin* Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/CompositionPlugins/

cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*MySql*   Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Npgsql*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Sqlite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*SQLite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Oracle*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*DB2*     Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/CompositionPlugins/

cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*MySql*   Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Npgsql*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Sqlite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*SQLite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*Oracle*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.3.x/bin/Debug/netcoreapp3.0/*DB2*     Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/

cd Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.0/
dotnet MsSqlCodeDiffVersioning.3x.dll $1
