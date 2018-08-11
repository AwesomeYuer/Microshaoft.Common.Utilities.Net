#if !NETFRAMEWORK4_X && NETCOREAPP2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Linq.Dynamic;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Net.Http.Formatting;
    using Microshaoft;
    using System.IO;
    using System.Composition.Convention;
    using System.Composition.Hosting;
    using System.Composition;
    using System.Collections.Generic;

    [Route("api/[controller]")]
    [ApiController]
    public abstract partial class 
                AbstractStoreProcedureExecutorControllerBase
                    :
                        ControllerBase //, IConnectionString
    {
        
        public AbstractStoreProcedureExecutorControllerBase()
        {
            if (_connections == null)
            {
                lock (_locker)
                {
                    if (_connections == null)
                    {
                        _connections
                            = GetDataBasesConnectionsInfo()
                                    .ToDictionary
                                        (
                                            (x) =>
                                            {
                                                return
                                                    x.ConnectionID;
                                            }
                                            , StringComparer.OrdinalIgnoreCase
                                        );
                    }
                }
            }

            //if
            //    (
            //        SqlHelper
            //            .CachedExecutingParametersExpiredInSeconds
            //        !=
            //        CachedExecutingParametersExpiredInSeconds
            //    )
            //{
            //    SqlHelper
            //            .CachedExecutingParametersExpiredInSeconds
            //                = CachedExecutingParametersExpiredInSeconds;
            //}
            if 
                (
                    _executors == null
                )
            {
                lock (_locker)
                {
                    if (_executors == null)
                    {
                        if (DynamicLoadExecutorsPaths != null)
                        {
                            var q
                                = DynamicLoadExecutorsPaths
                                        .Where
                                            (x => Directory.Exists(x))
                                        .SelectMany
                                            (
                                                (x) =>
                                                {
                                                    var r =
                                                        CompositionHelper
                                                            .ImportManyExportsComposeParts
                                                                <IStoreProcedureExecutable>
                                                                    (
                                                                        x
                                                                    );
                                                    return
                                                        r;
                                                }
                                            ).ToArray();
                            _executors = q
                                        .ToDictionary
                                            (
                                                (x) =>
                                                {
                                                    return
                                                        x.DataBaseType;
                                                }
                                                ,
                                                (x) =>
                                                {
                                                    IStoreProcedureParametersSetCacheAutoRefreshable
                                                        rr = x as IStoreProcedureParametersSetCacheAutoRefreshable;
                                                    if (rr != null)
                                                    {
                                                        rr.CachedExecutingParametersExpiredInSeconds
                                                            = CachedExecutingParametersExpiredInSeconds;
                                                        rr.NeedAutoRefreshExecutedTimeForSlideExpire
                                                            = NeedAutoRefreshExecutedTimeForSlideExpire;
                                                    }
                                                    return
                                                        x;
                                                }
                                                , StringComparer.OrdinalIgnoreCase
                                            );



                            //if (Directory.Exists(DynamicLoadExecutorsPath))
                            //{
                            //    var r = CompositionHelper
                            //                .ImportManyExportsComposeParts<IStoreProcedureExecutable>
                            //                    (
                            //                        DynamicLoadExecutorsPath
                            //                    );

                            //    _executors = r.ToDictionary
                            //        (
                            //            (x) =>
                            //            {
                            //                return
                            //                    x.DataBaseType;
                            //            }
                            //            ,
                            //            (x) =>
                            //            {
                            //                IStoreProcedureParametersSetCacheAutoRefreshable
                            //                    rr = x as IStoreProcedureParametersSetCacheAutoRefreshable;
                            //                if (rr != null)
                            //                {
                            //                    rr.CachedExecutingParametersExpiredInSeconds
                            //                        = CachedExecutingParametersExpiredInSeconds;
                            //                    rr.NeedAutoRefreshExecutedTimeForSlideExpire
                            //                        = NeedAutoRefreshExecutedTimeForSlideExpire;
                            //                }
                            //                return
                            //                    x;

                            //            }
                            //            , StringComparer.OrdinalIgnoreCase
                            //        );
                            //}
                        }

                    }
                    
                }
            }
        }
        //[ResponseCache(Duration = 10)]
        //[
        //    TypeFilter
        //        (
        //            typeof(RouteAuthorizeActionFilter)
        //            //, IsReusable = false
        //            , Arguments = new object[] {  
        //                new string[]
        //                {
        //                    "storeProcedureName"
        //                }
        //            }
        //        )
        //]
        //[
        //    RouteAuthorizeActionFilter
        //    (
        //        new string[]
        //            {
        //                "storeProcedureName"
        //            }
        //    )
        //]
        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        [Route
            (
                "{connectionID}/"
                + "{storeProcedureName}/"
                + "{resultPathSegment1?}/"
                + "{resultPathSegment2?}/"
                + "{resultPathSegment3?}/"
                + "{resultPathSegment4?}/"
                + "{resultPathSegment5?}/"
                + "{resultPathSegment6?}"
            )
        ]
        public virtual ActionResult<JToken> ProcessActionRequest
                            (

                                [FromRoute]
                                string connectionID //= "mssql"

                                ,
                                [FromRoute]
                                string storeProcedureName

                                ,
                                [ModelBinder(typeof(JTokenModelBinder))]
                                JToken parameters = null
                                ,
                                [FromRoute]
                                string resultPathSegment1 = null
                                ,
                                [FromRoute]
                                string resultPathSegment2 = null
                                ,
                                [FromRoute]
                                string resultPathSegment3 = null
                                ,
                                [FromRoute]
                                string resultPathSegment4 = null
                                ,
                                [FromRoute]
                                string resultPathSegment5 = null
                                ,
                                [FromRoute]
                                string resultPathSegment6 = null

                            )
        {
            //string dataBaseType = "mssql";
            JToken result = null;
            var r = false;
            DataBaseConnectionInfo connectionInfo = null;
            r = _connections
                        .TryGetValue
                            (
                                connectionID
                                , out connectionInfo
                            );
            if (r)
            {
                r = Process
                        (
                            connectionInfo
                            , storeProcedureName
                            , Request.Method
                            , parameters
                            , out result
                        );
            }
            if (!r)
            {
                return StatusCode(403);
            }
            result["TimeStamp"] = DateTime.Now;
            result = result
                        .GetDescendantByPath
                            (
                                resultPathSegment1
                                , resultPathSegment2
                                , resultPathSegment3
                                , resultPathSegment4
                                , resultPathSegment5
                                , resultPathSegment6
                            );
            return result;
        }
    }
}
#endif