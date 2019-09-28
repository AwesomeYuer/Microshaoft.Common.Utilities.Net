rem only for Windows/dos cmd
            
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MsSQL.Plugin\MsSQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*plugin* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*plugin* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\JTokenModelParameterValidatorsPlugins\SamplePlugin\SamplePlugin.3.x\bin\Debug\netcoreapp3.0\*plugin* $(TargetDir)CompositionPlugins\ /Y

xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*mysql.data* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*npgsql* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*sqlite* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*oracle* $(TargetDir)CompositionPlugins\ /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*db2* $(TargetDir)CompositionPlugins\ /Y

xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*mysql.data* $(TargetDir) /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*npgsql* $(TargetDir) /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*sqlite* $(TargetDir) /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*oracle* $(TargetDir) /Y
xcopy ..\..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.Plugin\MySQL.Plugin.3.x\bin\Debug\netcoreapp3.0\*db2* $(TargetDir) /Y
