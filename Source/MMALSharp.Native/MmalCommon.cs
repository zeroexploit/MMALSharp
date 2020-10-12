﻿using System.Runtime.InteropServices;

namespace MMALSharp.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MMAL_CORE_STATISTICS_T
    {
        uint bufferCount, firstBufferTime, lastBufferTime, maxDelay;

        public uint BufferCount => bufferCount;
        public uint FirstBufferTime => firstBufferTime;
        public uint LastBufferTime => lastBufferTime;
        public uint MaxDelay => maxDelay;

        public MMAL_CORE_STATISTICS_T(uint bufferCount, uint firstBufferTime, uint lastBufferTime, uint maxDelay)
        {
            this.bufferCount = bufferCount;
            this.firstBufferTime = firstBufferTime;
            this.lastBufferTime = lastBufferTime;
            this.maxDelay = maxDelay;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MMAL_CORE_PORT_STATISTICS_T
    {
        MMAL_CORE_STATISTICS_T rx, tx;

        public MMAL_CORE_STATISTICS_T Rx => rx;
        public MMAL_CORE_STATISTICS_T Tx => tx;

        public MMAL_CORE_PORT_STATISTICS_T(MMAL_CORE_STATISTICS_T rx, MMAL_CORE_STATISTICS_T tx)
        {
            this.rx = rx;
            this.tx = tx;
        }
    }
}