﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    public enum ResizeOperationStatus : int { IN_PROGRESS, DONE };

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public unsafe struct ResizeInfo
    {
        [FieldOffset(0)]
        public ResizeOperationStatus status;

        [FieldOffset(4)]
        public int version;

        [FieldOffset(0)]
        public long word;
    }

    public enum Phase : int {
        PREP_INDEX_CHECKPOINT, INDEX_CHECKPOINT,
        PREPARE, IN_PROGRESS,
        WAIT_PENDING, WAIT_FLUSH,
        REST,
        PERSISTENCE_CALLBACK,
        GC,
        PREPARE_GROW, IN_PROGRESS_GROW,
        INTERMEDIATE,
    };

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public unsafe struct SystemState
    {
        [FieldOffset(0)]
        public Phase phase;

        [FieldOffset(4)]
        public int version;

        [FieldOffset(0)]
        public long word;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SystemState Copy(ref SystemState other)
        {
            var info = default(SystemState);
            info.word = other.word;
            return info;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SystemState Make(Phase status, int version)
        {
            var info = default(SystemState);
            info.phase = status;
            info.version = version;
            return info;
        }

    }

}
