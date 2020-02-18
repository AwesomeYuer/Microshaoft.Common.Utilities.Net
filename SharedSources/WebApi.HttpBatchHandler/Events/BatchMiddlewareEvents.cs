﻿#if NETCOREAPP
namespace Microshaoft.HttpBatchHandler.Events
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    public class BatchMiddlewareEvents
    {
        /// <summary>
        ///     After all batch request are executed
        /// </summary>
        public Func<BatchEndContext, CancellationToken, Task> OnBatchEndAsync { get; set; } =
            (context, token) => Task.CompletedTask;

        /// <summary>
        ///     Before an individual request is created
        /// </summary>
        public Func<BatchRequestPreparationContext, CancellationToken, Task> OnBatchPreparationAsync { get; set; } =
            (context, token) => Task.CompletedTask;

        /// <summary>
        ///     After an individual request in a batch is executed
        /// </summary>
        public Func<BatchRequestExecutedContext, CancellationToken, Task> OnBatchRequestExecutedAsync { get; set; } =
            (context, token) => Task.CompletedTask;

        /// <summary>
        ///     Before an individual request in a batch is executed
        /// </summary>
        public Func<BatchRequestExecutingContext, CancellationToken, Task> OnBatchRequestExecutingAsync { get; set; } =
            (context, token) => Task.CompletedTask;

        /// <summary>
        ///     Before any request in a batch are executed
        /// </summary>
        public Func<BatchStartContext, CancellationToken, Task> OnBatchStartAsync { get; set; } =
            (context, token) => Task.CompletedTask;

        /// <summary>
        ///     After all batch request are executed
        /// </summary>
        public virtual Task BatchEndAsync(BatchEndContext context, CancellationToken cancellationToken = default) => OnBatchEndAsync(context, cancellationToken);

        /// <summary>
        ///     After an individual request in a batch is executed
        /// </summary>
        public virtual Task BatchRequestExecutedAsync(BatchRequestExecutedContext context,
            CancellationToken cancellationToken = default) => OnBatchRequestExecutedAsync(context, cancellationToken);

        /// <summary>
        ///     Before an individual request in a batch is executed
        /// </summary>
        public virtual Task BatchRequestExecutingAsync(BatchRequestExecutingContext context,
            CancellationToken cancellationToken = default) => OnBatchRequestExecutingAsync(context, cancellationToken);

        /// <summary>
        ///     Before an individual request in a batch is executed
        /// </summary>
        public virtual Task BatchRequestPreparationAsync(BatchRequestPreparationContext context,
            CancellationToken cancellationToken = default) => OnBatchPreparationAsync(context, cancellationToken);

        /// <summary>
        ///     Before any request in a batch are executed
        /// </summary>
        public virtual Task BatchStartAsync(BatchStartContext context, CancellationToken cancellationToken = default) => OnBatchStartAsync(context, cancellationToken);
    }
}
#endif