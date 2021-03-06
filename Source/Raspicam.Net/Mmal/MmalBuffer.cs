﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Raspicam.Net.Extensions;
using Raspicam.Net.Native.Buffer;
using Raspicam.Net.Native.Util;
using Raspicam.Net.Utility;
using static Raspicam.Net.MmalNativeExceptionHelper;

namespace Raspicam.Net.Mmal
{
    unsafe class MmalBuffer : MmalObject, IBuffer
    {
        public uint Cmd => Ptr->Cmd;
        public uint AllocSize => Ptr->AllocSize;
        public uint Length => Ptr->Length;
        public uint Offset => Ptr->Offset;
        public uint Flags => Ptr->Flags;
        public long Pts => Ptr->Pts;
        public long Dts => Ptr->Dts;
        public MmalBufferHeaderTypeSpecific Type => Marshal.PtrToStructure<MmalBufferHeaderTypeSpecific>(Ptr->Type);
        public List<MmalBufferProperties> Properties { get; }
        public List<int> Events { get; }
        public MmalBufferHeader* Ptr { get; }

        public MmalBuffer(MmalBufferHeader* ptr)
        {
            Ptr = ptr;
            Properties = new List<MmalBufferProperties>();
            Events = new List<int>();
        }

        public bool AssertProperty(MmalBufferProperties property) => ((int)Flags & (int)property) == (int)property;

        public override string ToString()
        {
            InitialiseProperties();

            var sb = new StringBuilder();

            sb.Append(
                "\r\n Buffer Header \r\n" +
                "---------------- \r\n" +
                $"Length: {Length} \r\n" +
                $"Presentation Timestamp: {Pts} \r\n" +
                "Flags: \r\n");

            foreach (var prop in Properties)
                sb.Append($"{prop} \r\n");

            sb.Append("---------------- \r\n");

            return sb.ToString();
        }

        public override bool CheckState() => Ptr != null && (IntPtr)Ptr != IntPtr.Zero;

        public byte[] GetBufferData()
        {
            MmalCheck(Native.Buffer.MmalBuffer.MemLock(Ptr), "Unable to lock buffer header.");

            try
            {
                var ps = Ptr->Data + Offset;
                var buffer = new byte[(int)Ptr->Length];
                Marshal.Copy((IntPtr)ps, buffer, 0, buffer.Length);
                Native.Buffer.MmalBuffer.MemUnlock(Ptr);

                return buffer;
            }
            catch
            {
                // If something goes wrong, unlock the header.
                Native.Buffer.MmalBuffer.MemUnlock(Ptr);
                MmalLog.Logger.LogWarning("Unable to handle data. Returning null.");
                return null;
            }
        }

        public void ReadIntoBuffer(byte[] source, int length, bool eof)
        {
            Ptr->Length = (uint)length;
            Ptr->Dts = Ptr->Pts = MmalUtil.MmalTimeUnknown;
            Ptr->Offset = 0;

            if (eof)
                Ptr->Flags = (uint)MmalBufferProperties.MmalBufferHeaderFlagEos;

            Marshal.Copy(source, 0, (IntPtr)Ptr->Data, length);
        }

        public void Acquire()
        {
            if (CheckState())
                Native.Buffer.MmalBuffer.HeaderAcquire(Ptr);
        }

        public void Release()
        {
            if (CheckState())
                Native.Buffer.MmalBuffer.HeaderRelease(Ptr);
            else
                MmalLog.Logger.LogWarning("Buffer null, could not release.");

            Dispose();
        }

        public void Reset()
        {
            if (CheckState())
                Native.Buffer.MmalBuffer.HeaderReset(Ptr);
        }

        void InitialiseProperties()
        {
            Properties.Clear();

            if (!CheckState())
                return;

            var availableFlags = Enum.GetValues(typeof(MmalBufferProperties)).Cast<MmalBufferProperties>();
            foreach (var flag in availableFlags)
            {
                if (Flags.HasFlag(flag))
                    Properties.Add(flag);
            }
        }
    }
}
