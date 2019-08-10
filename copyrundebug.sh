dotnet build CommonUtilities.NET.Core.Standard.sln -c Debug

cp StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*Plugin*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*Plugin*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp JTokenModelParameterValidatorsPlugins/JTokenModelParameterValidatorSamplePlugin/bin/Debug/netcoreapp2.2/*Plugin* Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/

cp -rf Samples/MsSqlCodeDiffVersioning/wwwroot/* Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/wwwroot/

cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*MySql*   Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*Npgsql*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*Sqlite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*SQLite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*Oracle*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*DB2*     Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/CompositionPlugins/

cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*MySql*   Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*Npgsql*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*Sqlite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*SQLite*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*Oracle*  Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/
cp StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.2/*DB2*     Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/

dotnet Samples/MsSqlCodeDiffVersioning/bin/Debug/netcoreapp2.2/MsSqlCodeDiffVersioning.WebApplication.dll /wait

