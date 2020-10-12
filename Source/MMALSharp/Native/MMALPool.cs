﻿using System;
using System.Runtime.InteropServices;

namespace MMALSharp.Native
{
    public static class MmalPool
    {
        public delegate void MmalPoolAllocatorAllocT(IntPtr ptr, uint value);
        public delegate void MmalPoolAllocatorFreeT(IntPtr ptr, IntPtr ptr2);

        // typedef - Pointer to MMAL_POOL_T struct * Pointer to MMAL_BUFFER_HEADER_T struct * Pointer to void -> Returns MMAL_BOOL_T struct
        public unsafe delegate int MMAL_POOL_BH_CB_T(MMAL_POOL_T* pool, MMAL_BUFFER_HEADER_T* buffer);

        // MMAL_POOL_T*
        [DllImport("libmmal.so", EntryPoint = "mmal_pool_create", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr mmal_pool_create(uint bufferNum, uint bufferSize);

        // MMAL_POOL_T*
        [DllImport("libmmal.so", EntryPoint = "mmal_pool_create_with_allocator", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr mmal_pool_create_with_allocator(uint headers,
                                                            uint payload_size,
                                                            IntPtr allocator_context,
                                                            MmalPoolAllocatorAllocT allocator_alloc,
                                                            MmalPoolAllocatorFreeT allocator_free);

        [DllImport("libmmal.so", EntryPoint = "mmal_pool_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void mmal_pool_destroy(MMAL_POOL_T* pool);

        [DllImport("libmmal.so", EntryPoint = "mmal_pool_resize", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_pool_resize(MMAL_POOL_T* pool, uint headers, uint payload_size);
                
        [DllImport("libmmal.so", EntryPoint = "mmal_pool_callback_set", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void mmal_pool_callback_set(MMAL_POOL_T* pool, [MarshalAs(UnmanagedType.FunctionPtr)] MMAL_POOL_BH_CB_T cb, IntPtr userdata);

        [DllImport("libmmal.so", EntryPoint = "mmal_pool_pre_release_callback_set", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void mmal_pool_pre_release_callback_set(MMAL_POOL_T* pool, [MarshalAs(UnmanagedType.FunctionPtr)] MMAL_POOL_BH_CB_T cb, IntPtr userdata);
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MMAL_POOL_T
    {
        MMAL_QUEUE_T* queue;
        uint headersNum;
        IntPtr header;

        public MMAL_QUEUE_T* Queue => queue;
        public uint HeadersNum => headersNum;
        public IntPtr Header => header;

        public MMAL_POOL_T(MMAL_QUEUE_T* queue, uint headersNum, IntPtr header)
        {
            this.queue = queue;
            this.headersNum = headersNum;
            this.header = header;
        }
    }
}
