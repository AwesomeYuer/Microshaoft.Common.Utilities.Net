// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microshaoft.Extensions.Logging.Console
{
    using System;
    internal interface IConsole
    {
        void Write(string message, ConsoleColor? background, ConsoleColor? foreground);
        void WriteLine(string message, ConsoleColor? background, ConsoleColor? foreground);
        void Flush();
    }
}
