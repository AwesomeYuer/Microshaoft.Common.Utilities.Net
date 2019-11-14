#if NETCOREAPP3_X
namespace Microshaoft.Web
{
    using Microshaoft.AspNetCore.ConcurrencyLimiters;
    using Microsoft.AspNetCore.ConcurrencyLimiter;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Threading.Tasks;

    public enum ConcurrencyLimiterQueuePoliciesEnum
    {
        FIFOQueue
        , LIFOQueue

    }

    public class ConcurrencyLimiterFilterAttribute
                    :
                        Attribute
                        , IAsyncActionFilter
                       
    {
        private readonly IQueuePolicy _queuePolicy = null;
        public ConcurrencyLimiterFilter
                            (
                                ConcurrencyLimiterQueuePoliciesEnum
                                        policy = ConcurrencyLimiterQueuePoliciesEnum.FIFOQueue
                            )
        {
            if (policy == ConcurrencyLimiterQueuePoliciesEnum.LIFOQueue)
            {
                _queuePolicy = new LIFOQueuePolicy
                        (
                            new QueuePolicyOptions()
                            {
                                MaxConcurrentRequests = 1
                                , RequestQueueLimit = 1
                            }
                        );
            }
            else
            {
                _queuePolicy = new FIFOQueuePolicy
                            (
                                new QueuePolicyOptions()
                                {
                                    MaxConcurrentRequests = 1
                                    , RequestQueueLimit = 1
                                }
                            );
            }
            Initialize();
        }
        public virtual void Initialize()
        {
            //InstanceID = Interlocked.Increment(ref InstancesSeed);
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var waitInQueueTask = _queuePolicy.TryEnterAsync();
            // Make sure we only ever call GetResult once on the TryEnterAsync ValueTask b/c it resets.
            bool r;
            if (waitInQueueTask.IsCompleted)
            {
                //ConcurrencyLimiterEventSource.Log.QueueSkipped();
                r = waitInQueueTask.Result;
            }
            else
            {
                //using (ConcurrencyLimiterEventSource.Log.QueueTimer())
                {
                    r = await waitInQueueTask;
                }
            }
            if (r)
            {
                try
                {
                    await next();
                }
                finally
                {
                    _queuePolicy.OnExit();
                }
            }
            else
            {
                var result = new JsonResult
                                (
                                    new
                                    {
                                        statusCode = 503
                                        , message = "Concurrency Limited!"
                                    }
                                )
                {
                    StatusCode = 503
                    , ContentType = "application/json"
                };
                context
                        .Result = result;
            }
        }
    }
}
#endif