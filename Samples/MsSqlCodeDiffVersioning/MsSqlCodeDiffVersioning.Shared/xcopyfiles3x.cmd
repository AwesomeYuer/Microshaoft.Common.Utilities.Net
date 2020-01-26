rem only for Windows/dos cmd
            
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MsSQL\MsSQL.Plugin.NET.Standard.2.x\bin\Debug\netstandard2.1\Microshaoft*.dll $(TargetDir)Plugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\Microshaoft*.dll $(TargetDir)Plugins\ /Y
xcopy ..\..\..\Plugins\JsonParametersValidators\SamplePlugin\SamplePlugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\Microshaoft*.dll $(TargetDir)Plugins\ /Y

xcopy ..\..\..\Plugins\StoreProceduresExecutors\MsSQL\MsSQL.Plugin.NET.Standard.2.x\bin\Debug\netstandard2.1\Microshaoft*.pdb $(TargetDir)Plugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\Microshaoft*.pdb $(TargetDir)Plugins\ /Y
xcopy ..\..\..\Plugins\JsonParametersValidators\SamplePlugin\SamplePlugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\Microshaoft*.pdb $(TargetDir)Plugins\ /Y

xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*mysql.data* $(TargetDir)Plugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*npgsql* $(TargetDir)Plugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*sqlite* $(TargetDir)Plugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*oracle* $(TargetDir)Plugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*db2* $(TargetDir)Plugins\ /Y

xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*mysql.data* $(TargetDir) /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*npgsql* $(TargetDir) /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*sqlite* $(TargetDir) /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*oracle* $(TargetDir) /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutors\MySQL\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*db2* $(TargetDir) /Y
