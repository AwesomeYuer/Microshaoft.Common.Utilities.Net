rem only for Windows/dos cmd
            
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MsSQL.Plugin\MsSQL.Plugin.NET.Standard.2.x\bin\Debug\netstandard2.1\Microshaoft*.dll $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\Microshaoft*.dll $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\Plugins\JTokenModelParameterValidatorsPlugins\SamplePlugin\SamplePlugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\Microshaoft*.dll $(TargetDir)CompositionPlugins\ /Y

xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MsSQL.Plugin\MsSQL.Plugin.NET.Standard.2.x\bin\Debug\netstandard2.1\Microshaoft*.pdb $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\Microshaoft*.pdb $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\Plugins\JTokenModelParameterValidatorsPlugins\SamplePlugin\SamplePlugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\Microshaoft*.pdb $(TargetDir)CompositionPlugins\ /Y

xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*mysql.data* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*npgsql* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*sqlite* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*oracle* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*db2* $(TargetDir)CompositionPlugins\ /Y

xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*mysql.data* $(TargetDir) /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*npgsql* $(TargetDir) /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*sqlite* $(TargetDir) /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*oracle* $(TargetDir) /Y
xcopy ..\..\..\Plugins\StoreProceduresExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.NET.Core.3.x\bin\Debug\netcoreapp3.1\*db2* $(TargetDir) /Y
