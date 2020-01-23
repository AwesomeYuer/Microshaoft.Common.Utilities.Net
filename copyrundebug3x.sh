dotnet build Microshaoft.Common.Utilities.Net.sln -c Debug

mkdir -p Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/wwwroot/
cp -rf Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.Shared/wwwroot/* Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/wwwroot/

mkdir -p Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/RoutesConfig/
cp -rf Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/RoutesConfig/* Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/RoutesConfig/

mkdir -p Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/CompositionPlugins/
cp Plugins/StoreProceduresExecutorsPlugins/MsSQL.Plugin/MsSQL.Plugin.NET.Standard.2.x/bin/Debug/netstandard2.1/*Plugin*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/CompositionPlugins/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Plugin*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/CompositionPlugins/
cp Plugins/JTokenModelParameterValidatorsPlugins/SamplePlugin/SamplePlugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Plugin* Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/CompositionPlugins/

cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*MySql*   Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/CompositionPlugins/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Npgsql*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/CompositionPlugins/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Sqlite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/CompositionPlugins/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*SQLite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/CompositionPlugins/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Oracle*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/CompositionPlugins/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*DB2*     Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/CompositionPlugins/

cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*MySql*   Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Npgsql*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Sqlite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*SQLite*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Oracle*  Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/
cp Plugins/StoreProceduresExecutorsPlugins/MySQL.Plugin/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*DB2*     Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/

cd Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.3x/bin/Debug/netcoreapp3.1/
dotnet MsSqlCodeDiffVersioning.3x.dll $1
