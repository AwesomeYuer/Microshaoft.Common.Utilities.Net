// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
namespace Microshaoft
{
    using System;
    using System.Reflection;

    public sealed class WeakEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly WeakReference _targetWeakReference;
        private readonly MethodInfo _method;

        public WeakEventHandler(EventHandler<TEventArgs> handler, bool trackResurrection = false)
        {
            _method = handler.Method;
            _targetWeakReference = new WeakReference(handler.Target, trackResurrection);
        }

        //[DebuggerNonUserCode]
        public void Handler(object sender, TEventArgs e)
        {
            var target = _targetWeakReference.Target;
            if (target != null)
            {
                (
                    (Action<object, TEventArgs>)
                        Delegate
                            .CreateDelegate
                                    (
                                        typeof(Action<object, TEventArgs>)
                                        , target
                                        , _method
                                        , true
                                    )
                )?.Invoke(sender, e);
            }
        }
    }
}