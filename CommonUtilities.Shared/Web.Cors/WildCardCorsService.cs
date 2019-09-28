#if NETCOREAPP2_X

namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// 一劳永逸：域名支持通配符，ASP.NET Core中配置CORS
    /// http://www.cnblogs.com/dudu/p/5895424.html
    /// </summary>
    public class WildcardCorsService : CorsService
    {
        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="options"></param>
        [Obsolete]
        public WildcardCorsService(IOptions<CorsOptions> options, ILoggerFactory loggerFactory) : base(options, loggerFactory)
        {

        }

        public override void EvaluateRequest(HttpContext context, CorsPolicy policy, CorsResult result)
        {
            var origin = context.Request.Headers[CorsConstants.Origin];
            EvaluateOriginForWildcard(policy.Origins, origin);
            base.EvaluateRequest(context, policy, result);
        }

        public override void EvaluatePreflightRequest(HttpContext context, CorsPolicy policy, CorsResult result)
        {
            var origin = context.Request.Headers[CorsConstants.Origin];
            EvaluateOriginForWildcard(policy.Origins, origin);
            base.EvaluatePreflightRequest(context, policy, result);
        }

        private void EvaluateOriginForWildcard(IList<string> origins, string origin)
        {
            //只在没有匹配的origin的情况下进行操作
            if (!origins.Contains(origin))
            {
                //查询所有以星号开头的origin
                var wildcardDomains = origins.Where(o => o.StartsWith("*"));
                if (wildcardDomains.Any())
                {
                    //遍历以星号开头的origin
                    foreach (var wildcardDomain in wildcardDomains)
                    {
                        //如果以.cnblogs.com结尾
                        if 
                            (
                                origin.EndsWith(wildcardDomain.Substring(1))
                                //或者以//cnblogs.com结尾，针对http://cnblogs.com
                                ||
                                origin.EndsWith("//" + wildcardDomain.Substring(2))
                            )
                        {
                            //将http://www.cnblogs.com添加至origins
                            origins.Add(origin);
                            break;
                        }
                    }
                }
            }
        }
    }
}


#endif