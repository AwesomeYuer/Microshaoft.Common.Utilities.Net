using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microshaoft.Net;

[assembly: AssemblyDescription(AssemblyInfoManager.AssemblyDescription)]

namespace Microshaoft.Net
{
    using System;
    using System.Reflection;
    public static class AssemblyInfoManager
    {

        public const string AssemblyDescription =
#if NET35
            "for .NET 3.5"
#elif NET45
            "for .NET 4.5.1"
#else
            "Microshaoft.Net for .NET"
#endif
;
    }
}
