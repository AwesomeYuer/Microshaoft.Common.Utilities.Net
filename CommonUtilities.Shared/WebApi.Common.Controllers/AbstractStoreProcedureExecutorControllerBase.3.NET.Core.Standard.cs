#if !NETFRAMEWORK4_X && NETCOREAPP2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Linq.Dynamic;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Linq;
    [Route("api/[controller]")]
    [ApiController]
    public abstract partial class 
                AbstractStoreProcedureExecutorControllerBase
                    :
                        ControllerBase
    {
        public AbstractStoreProcedureExecutorControllerBase()
        {
            _locker
                .LockIf
                    (
                        () =>
                        {
                            var r = (_connections == null);
                            return r;
                        }
                        , () =>
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
                                                , StringComparer
                                                        .OrdinalIgnoreCase
                                            );
                        }
                    );
            _locker
                .LockIf
                    (
                        () =>
                        {
                            var r = (_executors == null);
                            return r;
                        }
                        , () =>
                        {
                            if (DynamicLoadExecutorsPaths != null)
                            {
                                var q =
                                    DynamicLoadExecutorsPaths
                                        .Where
                                            (
                                                (x) =>
                                                {
                                                    return
                                                        Directory
                                                            .Exists(x);
                                                }
                                            )
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
                                            );
                                _executors =
                                    q
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
                                            , StringComparer
                                                    .OrdinalIgnoreCase
                                        );
                            }
                        }
                    );
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
        [
            Route
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
            var beginTime = DateTime.Now;
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
                            , CommandTimeoutInSeconds
                        );
            }
            if (!r)
            {
                return StatusCode(403);
            }
            result["BeginTime"] = beginTime;
            var endTime = DateTime.Now;
            result["EndTime"] = endTime;
            result["DurationInMilliseconds"]
                    = DateTimeHelper
                            .MillisecondsDiff
                                    (
                                        beginTime
                                        , endTime
                                    );
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
