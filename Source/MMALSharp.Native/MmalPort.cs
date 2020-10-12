﻿using System;
using System.Runtime.InteropServices;
using MMALSharp.Native.Buffer;
using MMALSharp.Native.Component;
using MMALSharp.Native.Format;

namespace MMALSharp.Native
{
    public static class MmalPort
    {
        public enum MmalPortTypeT
        {
            MmalPortTypeUnknown,
            MmalPortTypeControl,
            MmalPortTypeInput,
            MmalPortTypeOutput,
            MmalPortTypeClock
        }

        public const uint MmalPortTypeInvalid = 0xffffffff;
        public const int MmalPortCapabilityPassthrough = 0x01;
        public const int MmalPortCapabilityAllocation = 0x02;
        public const int MmalPortCapabilitySupportsEventFormatChange = 0x04;

        // MMAL_PORT_T* port    
        [DllImport("libmmal.so", EntryPoint = "mmal_port_format_commit", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_port_format_commit(MMAL_PORT_T* port);

        // typedef - Pointer to MMAL_PORT_T * Pointer to MMAL_BUFFER_HEADER_T -> Returns void
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void MMAL_PORT_BH_CB_T(MMAL_PORT_T* port, MmalBufferHeader* buffer);

        // MMAL_PORT_T* port * MMAL_PORT_BH_CB_T cb -> Returns MMAL_STATUS_T
        [DllImport("libmmal.so", EntryPoint = "mmal_port_enable", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_port_enable(MMAL_PORT_T* port, IntPtr cb);

        [DllImport("libmmal.so", EntryPoint = "mmal_wrapper_port_enable", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_wrapper_port_enable(MMAL_PORT_T* port, uint flags);
        
        // MMAL_PORT_T* port -> Returns MMAL_STATUS_T
        [DllImport("libmmal.so", EntryPoint = "mmal_port_disable", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_port_disable(MMAL_PORT_T* port);

        [DllImport("libmmal.so", EntryPoint = "mmal_wrapper_port_disable", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_wrapper_port_disable(MMAL_PORT_T* port);

        // MMAL_PORT_T* port -> Returns MMAL_STATUS_T
        [DllImport("libmmal.so", EntryPoint = "mmal_port_flush", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_port_flush(MMAL_PORT_T* port);

        // MMAL_PORT_T* port * MMAL_PARAMETER_HEADER_T* header
        [DllImport("libmmal.so", EntryPoint = "mmal_port_parameter_set", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_port_parameter_set(MMAL_PORT_T* port, MMAL_PARAMETER_HEADER_T* header);

        // MMAL_PORT_T* port * MMAL_PARAMETER_HEADER_T* header
        [DllImport("libmmal.so", EntryPoint = "mmal_port_parameter_get", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_port_parameter_get(MMAL_PORT_T* port, MMAL_PARAMETER_HEADER_T* header);

        // MMAL_PORT_T* port * MMAL_BUFFER_HEADER_T* header
        [DllImport("libmmal.so", EntryPoint = "mmal_port_send_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_port_send_buffer(MMAL_PORT_T* port, MmalBufferHeader* header);

        // MMAL_PORT_T* port * MMAL_PORT_T* port2
        [DllImport("libmmal.so", EntryPoint = "mmal_port_connect", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_port_connect(MMAL_PORT_T* port, MMAL_PORT_T* port2);

        // MMAL_PORT_T* port
        [DllImport("libmmal.so", EntryPoint = "mmal_port_disconnect", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_port_disconnect(MMAL_PORT_T* port);

        // MMAL_PORT_T* port * UInt32 payload_size 
        [DllImport("libmmal.so", EntryPoint = "mmal_port_payload_alloc", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe byte* mmal_port_payload_alloc(MMAL_PORT_T* port, [In] uint payload_size);

        // MMAL_PORT_T* port * uint8* payload_size
        [DllImport("libmmal.so", EntryPoint = "mmal_port_payload_free", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void mmal_port_payload_free(MMAL_PORT_T* port, [In] ref byte payload_size);

        // MMAL_PORT_T* port * MMAL_BUFFER_HEADER_T** buffer
        [DllImport("libmmal.so", EntryPoint = "mmal_port_event_get", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MmalUtil.MmalStatusT mmal_port_event_get(MMAL_PORT_T* port, IntPtr* buffer);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MMAL_PORT_PRIVATE_T
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MMAL_PORT_T
    {
        IntPtr priv;
        char* name;
        MmalPort.MmalPortTypeT type;
        ushort index, indexAll;
        int isEnabled;
        MmalEsFormat* format;
        int bufferNumMin, bufferSizeMin, bufferAlignmentMin, bufferNumRecommended, bufferSizeRecommended, bufferNum, bufferSize;
        MmalComponentType* component;
        IntPtr userData;
        uint capabilities;

        public IntPtr Priv => priv;
        public char* Name => name;
        public MmalPort.MmalPortTypeT Type => type;
        public ushort Index => index;
        public ushort IndexAll => indexAll;
        public int IsEnabled => isEnabled;
        public MmalEsFormat* Format => format;
        public int BufferNumMin => bufferNumMin;
        public int BufferSizeMin => bufferSizeMin;
        public int BufferAlignmentMin => bufferAlignmentMin;
        public int BufferNumRecommended => bufferNumRecommended;
        public int BufferSizeRecommended => bufferSizeRecommended;

        public int BufferNum
        {
            get
            {
                return this.bufferNum;
            }
            set
            {
                this.bufferNum = value;
            }
        }

        public int BufferSize
        {
            get
            {
                return this.bufferSize;
            }
            set
            {
                this.bufferSize = value;
            }
        }

        public MmalComponentType* Component => component;
        public IntPtr UserData => userData;
        public uint Capabilities => capabilities;

        public MMAL_PORT_T(IntPtr priv, char* name, MmalPort.MmalPortTypeT type, ushort index, ushort indexAll,
                           int isEnabled, MmalEsFormat* format, int bufferNumMin, int bufferSizeMin, int bufferAlignmentMin,
                           int bufferNumRecommended, int bufferSizeRecommended, int bufferNum, int bufferSize, MmalComponentType* component,
                           IntPtr userData, uint capabilities)
        {
            this.priv = priv;
            this.name = name;
            this.type = type;
            this.index = index;
            this.indexAll = indexAll;
            this.isEnabled = isEnabled;
            this.format = format;
            this.bufferNumMin = bufferNumMin;
            this.bufferSizeMin = bufferSizeMin;
            this.bufferAlignmentMin = bufferAlignmentMin;
            this.bufferNumRecommended = bufferNumRecommended;
            this.bufferSizeRecommended = bufferSizeRecommended;
            this.bufferNum = bufferNum;
            this.bufferSize = bufferSize;
            this.component = component;
            this.userData = userData;
            this.capabilities = capabilities;
        }
    }
}
