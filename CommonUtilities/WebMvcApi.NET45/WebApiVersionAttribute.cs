using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microshaoft.Web
{
    [
        AttributeUsage
            (
                AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter
                , AllowMultiple = false
                , Inherited = false
            )
    ]
    public class WebApiVersionAttribute : Attribute
    {
        public WebApiVersionAttribute(string version)
        {
            Version v = null;
            if (Version.TryParse(version, out v))
            {
                ApiVersionNumber = version;
                ApiVersion = v;
                
            }
            else
            {
                throw new Exception("无效的版本号");
            }
        }

        public string ApiVersionNumber = string.Empty;

        public Version ApiVersion = null;

    }
}
