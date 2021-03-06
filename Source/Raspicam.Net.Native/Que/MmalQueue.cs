﻿using System.Runtime.InteropServices;
using Raspicam.Net.Native.Buffer;

namespace Raspicam.Net.Native.Que
{
    public static class MmalQueue
    {
        [DllImport("libmmal.so", EntryPoint = "mmal_queue_put", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void Put(MmalQueueType* ptr, MmalBufferHeader* header);

        [DllImport("libmmal.so", EntryPoint = "mmal_queue_get", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalBufferHeader* Get(MmalQueueType* ptr);

        [DllImport("libmmal.so", EntryPoint = "mmal_queue_wait", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalBufferHeader* Wait(MmalQueueType* ptr);

        [DllImport("libmmal.so", EntryPoint = "mmal_queue_timedwait", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalBufferHeader* TimedWait(MmalQueueType* ptr, int waitms);

        [DllImport("libmmal.so", EntryPoint = "mmal_queue_length", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe uint Length(MmalQueueType* ptr);

        [DllImport("libmmal.so", EntryPoint = "mmal_queue_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void Destroy(MmalQueueType* ptr);
    }
}
