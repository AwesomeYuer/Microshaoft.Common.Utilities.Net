rem only for Windows/dos cmd
            
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MsSQL.Plugin\MsSQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*plugin* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*plugin* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\JTokenModelParameterValidatorsPlugins\SamplePlugin\SamplePlugin.2.x\bin\Debug\netcoreapp2.2\*plugin* $(TargetDir)CompositionPlugins\ /Y

xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*mysql.data* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*npgsql* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*sqlite* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*oracle* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*db2* $(TargetDir)CompositionPlugins\ /Y

xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*mysql.data* $(TargetDir) /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*npgsql* $(TargetDir) /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*sqlite* $(TargetDir) /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*oracle* $(TargetDir) /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.2.x\bin\Debug\netcoreapp2.2\*db2* $(TargetDir) /Y
