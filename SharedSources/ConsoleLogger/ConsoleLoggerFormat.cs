// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microshaoft.Extensions.Logging.Console
{
    /// <summary>
    /// Format of <see cref="LightConsoleLogger" /> messages.
    /// </summary>
    public enum ConsoleLoggerFormat
    {
        /// <summary>
        /// Produces messages in the default console format.
        /// </summary>
        Default,
        /// <summary>
        /// Produces messages in a format suitable for console output to the systemd journal.
        /// </summary>
        Systemd,
    }
}
