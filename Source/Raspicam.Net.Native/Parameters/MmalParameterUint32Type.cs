﻿using System.Runtime.InteropServices;

namespace Raspicam.Net.Native.Parameters
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MmalParameterUint32Type
    {
        public MmalParameterHeaderType Hdr;
        public uint Value;

        public MmalParameterUint32Type(MmalParameterHeaderType hdr, uint value)
        {
            Hdr = hdr;
            Value = value;
        }
    }
}