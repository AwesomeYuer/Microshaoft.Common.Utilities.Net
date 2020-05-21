﻿namespace Microshaoft
{
    // ------------------------------------------------------------------------------
    // <copyright file="TaskExtensions.cs" company="Microsoft Corporation">
    // Copyright (c) Microsoft Corporation. All rights reserved.
    // </copyright>
    // ------------------------------------------------------------------------------

    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions for the <see cref="Task"/> class.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Runs an action on the current scheduler instead of the default scheduler.
        /// </summary>
        /// <param name="this">Scheduler for the action to be scheduled on.</param>
        /// <param name="action">Action to be scheduled.</param>
        /// <param name="cancelationToken">Cancelation token to link the new task to. If canceled before being scheduled, the action will not be run.</param>
        /// <returns>New task created for the action.</returns>
        public static Task Run
                            (
                                this TaskScheduler @this
                                , Action action
                                , CancellationToken cancelationToken = default
                            )
        {
            var taskFactory = new TaskFactory
                                (
                                    CancellationToken.None
                                    , TaskCreationOptions.DenyChildAttach
                                    , TaskContinuationOptions.None
                                    , @this
                                );
            return
                taskFactory
                        .StartNew
                            (
                                action
                                , cancellationToken: cancelationToken
                            );
        }

        /// <summary>
        /// Runs a function on the current scheduler instead of the default scheduler.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="this">Scheduler for the action to be scheduled on.</param>
        /// <param name="processFunc">Function to be scheduled.</param>
        /// <param name="cancelationToken">Cancelation token to link the new task to. If canceled before being scheduled, the action will not be run.</param>
        /// <returns>New task created for the function. This task completes with the result of calling the function.</returns>
        public static Task<T> Run<T>
                                (
                                    this TaskScheduler @this
                                    , Func<T> processFunc
                                    , CancellationToken cancelationToken = default
                                )
        {
            var taskFactory = new TaskFactory
                                        (
                                            CancellationToken.None
                                            , TaskCreationOptions.DenyChildAttach
                                            , TaskContinuationOptions.None
                                            , @this
                                        );
            return
                taskFactory
                        .StartNew
                            (
                                processFunc
                                , cancellationToken: cancelationToken
                            );
        }

        /// <summary>
        /// Runs a function on the current scheduler instead of the default scheduler.
        /// </summary>
        /// <param name="this">Scheduler for the action to be scheduled on.</param>
        /// <param name="processFunc">Function to be scheduled.</param>
        /// <param name="cancelationToken">Cancelation token to link the new task to. If canceled before being scheduled, the action will not be run.</param>
        /// <returns>New task created for the function. This task completes with the result of the task returned by the function.</returns>
        public static async Task Run
                                    (
                                        this TaskScheduler @this
                                        , Func<Task> processFunc
                                        , CancellationToken cancelationToken = default
                                    )
        {
            var taskFactory = new TaskFactory
                                    (
                                        CancellationToken.None
                                        , TaskCreationOptions.DenyChildAttach
                                        , TaskContinuationOptions.None
                                        , @this
                                    );
            var innerTask = await
                                taskFactory
                                    .StartNew
                                        (
                                            processFunc
                                            , cancellationToken: cancelationToken
                                        );
            await innerTask;
        }

        /// <summary>
        /// Runs a function on the current scheduler instead of the default scheduler.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="this">Scheduler for the action to be scheduled on.</param>
        /// <param name="function">Function to be scheduled.</param>
        /// <param name="cancelationToken">Cancelation token to link the new task to. If canceled before being scheduled, the action will not be run.</param>
        /// <returns>New task created for the function. This task completes with the result of the task returned by the function.</returns>
        public static async Task<T> Run<T>
                                        (
                                            this TaskScheduler @this
                                            , Func<Task<T>> function
                                            , CancellationToken cancelationToken = default
                                        )
        {
            var taskFactory = new TaskFactory
                                    (
                                        CancellationToken.None
                                        , TaskCreationOptions.DenyChildAttach
                                        , TaskContinuationOptions.None
                                        , @this
                                    );
            var innerTask = await
                                taskFactory
                                    .StartNew
                                        (
                                            function
                                            , cancellationToken: cancelationToken
                                        );
            return await innerTask;
        }

        /// <summary>
        /// Returns a <see cref="SwitchSchedulerAwaiter"/>, which runs the continuation on the specified scheduler.
        /// </summary>
        /// <param name="this">Scheduler to resume execution on.</param>
        public static SwitchSchedulerAwaiter SwitchTo(this TaskScheduler @this)
        {
            return new SwitchSchedulerAwaiter(@this);
        }

        /// <summary>
        /// Custom awaiter that resumes the continuation on the specified scheduler.
        /// </summary>
        public struct SwitchSchedulerAwaiter : INotifyCompletion
        {
            private readonly TaskScheduler _scheduler;

            /// <summary>
            /// Initializes a new instance of the <see cref="SwitchSchedulerAwaiter"/> struct.
            /// </summary>
            public SwitchSchedulerAwaiter(TaskScheduler scheduler)
            {
                _scheduler = scheduler;
            }

            /// <summary>
            /// Whether the switch is completed.
            /// </summary>
            public bool IsCompleted => _scheduler == TaskScheduler.Current;

            /// <summary>
            /// Part of custom awaiter pattern.
            /// </summary>
            public void GetResult()
            {
            }

            /// <summary>
            /// Part of custom awaiter pattern.
            /// </summary>
            public SwitchSchedulerAwaiter GetAwaiter() => this;

            /// <inheritdoc/>
            public void OnCompleted(Action continuation)
            {
                _scheduler.Run(continuation);
            }
        }
    }
}