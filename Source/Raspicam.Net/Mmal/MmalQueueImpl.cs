﻿using System;
using Microsoft.Extensions.Logging;
using Raspicam.Net.Native.Que;
using Raspicam.Net.Utility;

namespace Raspicam.Net.Mmal
{
    unsafe class MmalQueueImpl : MmalObject, IBufferQueue
    {
        public MmalQueueType* Ptr { get; }

        public MmalQueueImpl(MmalQueueType* ptr)
        {
            Ptr = ptr;
        }

        public IBuffer GetBuffer()
        {
            var ptr = MmalQueue.Get(Ptr);

            if (!CheckState())
            {
                MmalLog.Logger.LogWarning("Buffer retrieved null.");
                return null;
            }

            return new MmalBuffer(ptr);
        }

        public override void Dispose()
        {
            MmalLog.Logger.LogDebug("Disposing queue.");
            Destroy();
            base.Dispose();
        }

        public override string ToString() => $"Ptr address: {(IntPtr)Ptr}";

        public override bool CheckState() => Ptr != null && (IntPtr)Ptr != IntPtr.Zero;

        public uint QueueLength() => MmalQueue.Length(Ptr);

        public IBuffer Wait() => new MmalBuffer(MmalQueue.Wait(Ptr));

        public IBuffer TimedWait(int waitms) => new MmalBuffer(MmalQueue.TimedWait(Ptr, waitms));

        public void Put(IBuffer buffer) => MmalQueue.Put(Ptr, buffer.Ptr);

        void Destroy() => MmalQueue.Destroy(Ptr);
    }
}
