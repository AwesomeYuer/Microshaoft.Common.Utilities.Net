/*
# Microshaoft
/r:System.Xaml.dll
/r:System.Activities.dll
/r:System.Activities.DurableInstancing.dll
/r:System.Runtime.DurableInstancing.dll
/r:"D:\Microshaoft.Nuget.Packages\Newtonsoft.Json.7.0.1\lib\net45\Newtonsoft.Json.dll"

import from 
https://github.com/maskx/SystemWorkflow/SystemWorkflow/SystemWorkflow/ActivityFactory.cs

*/
#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.XamlIntegration;
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Xaml;
    /// <summary>
    /// Create Instance DynamicActivity from XAML with CompileExpressions 
    /// </summary>
    public static class WorkFlowActivityFactory
    {
#region Member
        /// <summary>
        /// Compiled Expressions Type Cache
        /// </summary>
        static ConcurrentDictionary<string, Type> _TypeCache = new ConcurrentDictionary<string, Type>();
        static ConcurrentDictionary<string, Activity> _ActivityCache = new ConcurrentDictionary<string, Activity>();
        /// <summary>
        /// Object for lock, make one Expressions Type only be compiled once
        /// </summary>
        static object _LockObj = new object();
#endregion

        /// <summary>
        /// Get an DynamicActivity by Id and filename
        /// </summary>
        /// <param name="id">The workflow id</param>
        /// <param name="getFile">Get the workflow define file(.XAML) by Id</param>
        ///
        /// <returns>The Compiled DynamicActivity</returns>
        public static Activity Create(string id, Func<string, string> getFile, bool safe = true)
        {
            using (XamlXmlReader reader = new XamlXmlReader(getFile(id)))
            {
                return Create(id, reader, safe);
            }
        }
        /// <summary>
        /// Get an DynamicActivity by Id and  XamlXmlReader 
        /// </summary>
        /// <param name="id">The workflow id</param>
        /// <param name="reader">The XamlXmlReader defined workflow </param>
        /// <returns>The Compiled DynamicActivity</returns>
        static Activity Create(string id, XamlXmlReader reader, bool safe = true)
        {
            if (!safe)
                return GetActivityFromCache(id, reader);
            var activity = ActivityXamlServices.Load(reader);
            CompileExpressions(id, activity);
            return activity;
        }
        /// <summary>
        /// Get an DynamicActivity by Id and stream
        /// </summary>
        /// <param name="id">The workflow id</param>
        /// <param name="getStream">The stream defined workflow</param>
        /// <returns>The Compiled DynamicActivity</returns>
        public static Activity Create(string id, Func<string, Stream> getStream, bool safe = true)
        {
            using (XamlXmlReader reader = new XamlXmlReader(getStream(id)))
            {
                return Create(id, reader, safe);
            }
        }
        static Activity GetActivityFromCache(string id, XamlXmlReader reader)
        {
            if (!_ActivityCache.TryGetValue(id, out Activity act))
            {
                lock (_LockObj)
                {
                    if (_ActivityCache.TryGetValue(id, out act))
                        return act;
                    act = Create(id, reader, true);
                    _ActivityCache.TryAdd(id, act);
                }
            }
            return act;
        }
        /// <summary>
        /// Compile the Expressions in workflow
        /// </summary>
        /// <param name="id">The workflow Id</param>
        /// <param name="activity">The workflow activity for build the expressions</param>
        static void CompileExpressions(string id, Activity activity)
        {
            Type t = GetType(id, activity);
            ICompiledExpressionRoot compiledExpressionRoot =
               Activator.CreateInstance(t,
                   new object[] { activity }) as ICompiledExpressionRoot;
            CompiledExpressionInvoker.SetCompiledExpressionRootForImplementation(
                activity, compiledExpressionRoot);
        }
        /// <summary>
        /// Get Compiled Expressions type from activity 
        /// </summary>
        /// <param name="id">The workflow Id</param>
        /// <param name="activity">The activity for get expressions type</param>
        /// <returns>The Compiled Expressions type of activity</returns>
        static Type GetType(string id, Activity activity)
        {
            Contract.Ensures(Contract.Result<Type>() != null);
            if (!_TypeCache.TryGetValue(id, out Type t))
            {
                lock (_LockObj)
                {
                    if (_TypeCache.TryGetValue(id, out t))
                        return t;
                    t = GetType(activity as DynamicActivity);
                    _TypeCache.TryAdd(id, t);
                }
            }
            return t;
        }
        /// <summary>
        /// Compiled the activity and get the complied expressions type
        /// </summary>
        /// <param name="activity">The activity for compiled</param>
        /// <returns>The complied expressions type</returns>
        static Type GetType(DynamicActivity activity)
        {
            TextExpressionCompilerSettings settings = GetCompilerSettings(activity);
            TextExpressionCompilerResults results =
                new TextExpressionCompiler(settings).Compile();
            if (results.HasErrors)
            {
                throw new Exception("Compilation failed.");
            }
            return results.ResultType;
        }
        /// <summary>
        /// Get the CompilerSettings
        /// </summary>
        /// <param name="dynamicActivity">The activity</param>
        /// <returns>The CompilerSettings</returns>
        static TextExpressionCompilerSettings GetCompilerSettings(DynamicActivity dynamicActivity)
        {
            int num = dynamicActivity.Name.LastIndexOf('.');
            int length = dynamicActivity.Name.Length;
            string text = (num > 0) ? dynamicActivity.Name.Substring(num + 1) : dynamicActivity.Name;
            text += "_CompiledExpressionRoot";
            string activityNamespace = (num > 0) ? dynamicActivity.Name.Substring(0, num) : null;
            return new TextExpressionCompilerSettings
            {
                Activity = dynamicActivity,
                ActivityName = text,
                ActivityNamespace = activityNamespace,
                RootNamespace = null,
                GenerateAsPartialClass = false,
                AlwaysGenerateSource = true,//if false,sometime return null type after compile
                Language = "C#"
            };
        }
    }
}
#endif