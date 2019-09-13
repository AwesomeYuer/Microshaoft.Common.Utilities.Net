#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IdentityModel.Tokens.Jwt;
    //using Microsoft.IdentityModel.Tokens;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Claims;
    using System.Text;
    using System.Web;
    using System.Web.Http.Controllers;
    //using Microsoft.AspNetCore.Http;
    public static partial class JwtTokenHelper
    {

        //public static bool TryIssueWebToken
        //        (
        //            this HttpContextBase target
        //            , IEnumerable<Claim> claims
        //            //, out string secretTokenString
        //        )
        //{
        //    var secretTokenString = string.Empty;
        //    var r = TryIssueWebToken
        //                (
        //                    claims
        //                    , out secretTokenString
        //                );
        //    if (r)
        //    {
        //        target.SetCookie("WebUserToken", secretTokenString);
        //    }
        //    return r;
        //}
        //public static bool TryGetCurrentWebUser
        //                            (

        //                               this HttpActionContext context
        //                                , out ClaimsPrincipal claimsPrincipal
        //                                , bool needValidateIP = true
        //                                , int expiredInSeconds = -1
        //                                , string cookieName = "WebUserToken"
        //                            )
        //{
        //    var r = false;
        //    claimsPrincipal = null;
        //    var cookieHeader = context.Request.Headers.GetCookies(cookieName).FirstOrDefault();

        //    if (cookieHeader != null)
        //    {
        //        string ip = null;
        //        if (needValidateIP)
        //        {
        //            ip = context.Request.GetClientIPAddress();
        //        }
        //        r = TryGetCurrentWebUser
        //                (
        //                    cookieHeader[cookieName].Value
        //                    , out claimsPrincipal
        //                    , ip
        //                    , expiredInSeconds
        //                );
        //        if (!r)
        //        {
        //            context.Response.ClearCookie(cookieName);
        //            claimsPrincipal = null;
        //        }
        //    }
        //    return r;
        //}
        //public static bool TryGetCurrentWebUser
        //                            (
        //                                this HttpContextBase context
        //                                , out ClaimsPrincipal claimsPrincipal
        //                                , bool needValidateIP = true
        //                                , int expiredInSeconds = -1
        //                                , string cookieName = "WebUserToken"
        //                            )
        //{
        //    var r = false;
        //    claimsPrincipal = null;
        //    var cookie = context.Request.Cookies[cookieName];

        //    if (cookie != null)
        //    {
        //        string ip = null;
        //        if (needValidateIP)
        //        {
        //            ip = context.Request.UserHostAddress;
        //        }
        //        r = TryGetCurrentWebUser
        //                (
        //                    cookie.Value
        //                    , out claimsPrincipal
        //                    , ip
        //                    , expiredInSeconds
        //                );
        //        if (!r)
        //        {
        //            context.ClearCookie(cookieName);
        //            claimsPrincipal = null;
        //        }
        //    }
        //    return r;
        //}

        public static void ClearCookie(this HttpContextBase httpContext, string cookieName)
        {
            if (httpContext.Request.Cookies[cookieName] != null)
            {
                HttpCookie cookie = new HttpCookie(cookieName);
                cookie.Expires = DateTime.Now.AddDays(-1d);
                httpContext.Response.Cookies.Add(cookie);
            }
        }
        public static void SetCookie(this HttpContextBase httpContext, string cookieName, string cookieValue)
        {
                HttpCookie cookie = new HttpCookie(cookieName)
                {
                     Value = cookieValue
                };
                //cookie.Expires = DateTime.Now.AddDays(-1d);
                httpContext.Response.Cookies.Add(cookie);
            
        }

    }
}
#endif