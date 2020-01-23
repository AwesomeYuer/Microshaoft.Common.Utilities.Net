/*
 * https://github.com/aspnet/AspNetCore/tree/fece4705eec5b2a118d9bd8b68eb867d2f573f7c/src/Middleware/ConcurrencyLimiter
 * https://www.nuget.org/packages/Microsoft.AspNetCore.ConcurrencyLimiter/
 * above is Middleware only
 * Filter
 */


#if NETCOREAPP3_X
namespace Microshaoft.AspNetCore.ConcurrencyLimiters
{
    using Microsoft.AspNetCore.ConcurrencyLimiter;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading.Tasks;

    public enum QueueStoreTypeEnum
    {
        QueueFIFO
        , StackLIFO
    }

    public class ConcurrencyLimiterFilterAttribute
                    :
                        Attribute
                        , IAsyncActionFilter
                       
    {
        private IQueuePolicy _queuePolicy = null;
        private int _maxConcurrentRequests = 64;
        public int MaxConcurrentRequests
        {
            get => _maxConcurrentRequests;
            set => _maxConcurrentRequests = value;
        }

        private int _requestQueueLimit = 128;
        public int RequestQueueLimit
        {
            get => _requestQueueLimit;
            set => _requestQueueLimit = value;
        }
       
        private QueueStoreTypeEnum _queueStoreType;
        public QueueStoreTypeEnum QueueStoreType
        { 
            get => _queueStoreType;
            set => _queueStoreType = value;
        }

        public ConcurrencyLimiterFilterAttribute()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            IOptions<QueuePolicyOptions> queuePolicyOptions
                = Options
                        .Create
                            (
                                new QueuePolicyOptions()
                                        {
                                            MaxConcurrentRequests = _maxConcurrentRequests
                                            , RequestQueueLimit = _requestQueueLimit
                                        }
                            );
            if (_queueStoreType == QueueStoreTypeEnum.StackLIFO)
            {
                _queuePolicy = new StackPolicy
                                    (
                                        queuePolicyOptions
                                    );
            }
            else
            {
                _queuePolicy = new QueuePolicy
                                    (
                                        queuePolicyOptions
                                    );
            }
        }
        public async Task OnActionExecutionAsync
                                        (
                                            ActionExecutingContext context
                                            , ActionExecutionDelegate next
                                        )
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
                    r = await
                            waitInQueueTask;
                }
            }
            if (r)
            {
                try
                {
                    await
                        next();
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
                                        , message = $"Concurrency {Enum.GetName(typeof(QueueStoreTypeEnum), _queueStoreType)} Queue Limited!"
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