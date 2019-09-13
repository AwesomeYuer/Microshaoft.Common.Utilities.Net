#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microshaoft;
    using Microshaoft.WebApi.Controllers;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    public interface IActionCommonProcessable
    {
        (
            int StatusCode
            , JToken Result
        )
                Process
                     (
                        JToken parameters = null
                    );
    }
    public interface IActionProcessable : IActionCommonProcessable
    {
        string Key
        {
            get;
        }
    }
    public abstract class
                AbstractActionCommonProcessingService
                                : IActionCommonProcessable
    {

        private static object _locker = new object();
        private IDictionary<string, IActionProcessable>
            _processors;
        public AbstractActionCommonProcessingService()
        {
            Initialize();
        }
        //for override from derived class
        public virtual void Initialize()
        {
            
        }

        protected virtual string[] GetDynamicExecutorsPathsProcess
            (
                string dynamicLoadExecutorsPathsJsonFile
                            = "dynamicLoadExecutorsPaths.json"
            )
        {
            var configurationBuilder =
                        new ConfigurationBuilder()
                                .AddJsonFile(dynamicLoadExecutorsPathsJsonFile);
            var configuration = configurationBuilder.Build();
            var result =
                    configuration
                        .GetSection("DynamicLoadExecutorsPaths")
                        .AsEnumerable()
                        .Select
                            (
                                (x) =>
                                {
                                    return
                                        x.Value;
                                }
                            )
                        .ToArray();
            return result;
        }

        protected virtual void LoadDynamicExecutors
                        (
                            string dynamicLoadExecutorsPathsJsonFile = "dynamicLoadExecutorsPaths.json"
                        )
        {
            var processors =
                    GetDynamicExecutorsPathsProcess
                            (
                                dynamicLoadExecutorsPathsJsonFile
                            )
                        .Where
                            (
                                (x) =>
                                {
                                    return
                                        (
                                            !x
                                                .IsNullOrEmptyOrWhiteSpace()
                                            &&
                                            Directory
                                                .Exists(x)
                                        );
                                }
                            )
                        .SelectMany
                            (
                                (x) =>
                                {
                                    var r =
                                        CompositionHelper
                                            .ImportManyExportsComposeParts
                                                <IActionProcessable>
                                                    (x);
                                    return r;
                                }
                            )
                        .ToDictionary
                            (
                                (x) =>
                                {
                                    return
                                        x.Key;
                                }
                                ,
                                (x) =>
                                {
                                    return x;
                                }
                                , StringComparer
                                        .OrdinalIgnoreCase
                            );
            _locker
                .LockIf
                    (
                        () =>
                        {
                            var r = (_processors == null);
                            return r;
                        }
                        , () =>
                        {
                            _processors = processors;
                        }
                    );
        }
        public (int StatusCode, JToken Result) Process(JToken parameters = null)
        {
            throw new NotImplementedException();
        }
    }
}
#endif