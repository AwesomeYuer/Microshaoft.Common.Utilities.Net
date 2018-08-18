namespace Microshaoft.Web
{
    using Microshaoft.WebApi.Controllers;
    using System;
    using System.Collections.Generic;
    public class StoreProceduresExecuteService
                            : AbstractStoreProceduresService
    {
        protected override
                IEnumerable<DataBaseConnectionInfo>
                                DataBasesConnectionsInfo        
        {
            get
            {
                return
                    new List<DataBaseConnectionInfo>()
                        {
                             new DataBaseConnectionInfo()
                             {
                                  ConnectionID = "mssql1"
                                  , DataBaseType =  DataBasesType.MsSQL
                                  , ConnectionString =
                                        @"Initial Catalog=Test;Data Source=localhost;User=sa;Password=!@#123QWE"
                                  , WhiteList = new Dictionary<string, HttpMethodsFlags>
                                                      (StringComparer.OrdinalIgnoreCase)
                                                    {
                                                        {
                                                            "zsp_GetDatesAfter"
                                                            , HttpMethodsFlags.All
                                                                //HttpMethodsFlags.Get 
                                                                //| HttpMethodsFlags.Post
                                                        }
                                                        ,
                                                        {
                                                            "zsp_Test"
                                                            , HttpMethodsFlags.All
                                                                //HttpMethodsFlags.Get 
                                                                //| HttpMethodsFlags.Post
                                                        }
                                                    }
                             }
                             ,
                             new DataBaseConnectionInfo()
                             {
                                  ConnectionID = "mysql1"
                                  , DataBaseType =  DataBasesType.MySQL
                                  , ConnectionString =
                                        @"server=microshaoft-ubuntu-001.westus.cloudapp.azure.com;uid=root;pwd=withoutpassword;database=Test"
                                  , WhiteList = new Dictionary<string, HttpMethodsFlags>
                                                      (StringComparer.OrdinalIgnoreCase)
                                                    {
                                                        {
                                                            "zsp_GetDatesAfter"
                                                            , HttpMethodsFlags.All
                                                                //HttpMethodsFlags.Get 
                                                                //| HttpMethodsFlags.Post
                                                        }
                                                        ,
                                                        {
                                                            "zsp_Test"
                                                            , HttpMethodsFlags.All
                                                                //HttpMethodsFlags.Get 
                                                                //| HttpMethodsFlags.Post
                                                        }
                                                    }
                             }
                        };
            }
        }
        protected override
                    string[] DynamicLoadExecutorsPaths
        {
            get
            {
                return
                    new string[]
                        {
                            @"D:\MyGitHub\Microshaoft.Common.Utilities.Net.4x\StoreProcedureWebApiExecutorsPlugins\MsSQL.StoreProcedureWebApiExecutor.Plugin\bin\Debug\netcoreapp2.1\"
                            ,
                            @"D:\MyGitHub\Microshaoft.Common.Utilities.Net.4x\StoreProcedureWebApiExecutorsPlugins\MySQL.StoreProcedureWebApiExecutor.Plugin\bin\Debug\netcoreapp2.1"
                            ,
                            "/mnt/d/MyGitHub/Microshaoft.Common.Utilities.Net.4x/StoreProcedureWebApiExecutorsPlugins/MsSQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.1/"
                            ,
                            "/mnt/d/MyGitHub/Microshaoft.Common.Utilities.Net.4x/StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.1"
                            ,
                            "/home/microshaoft/mygithub/Microshaoft.Common.Utilities.Net.4x/StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.1/"
                            ,
                            "/home/microshaoft/mygithub/Microshaoft.Common.Utilities.Net.4x/StoreProcedureWebApiExecutorsPlugins/MySQL.StoreProcedureWebApiExecutor.Plugin/bin/Debug/netcoreapp2.1/"
                        };
            }
        }
        protected override int
                CachedExecutingParametersExpiredInSeconds => 11;
        protected override bool 
                NeedAutoRefreshExecutedTimeForSlideExpire => true;
    }
}
