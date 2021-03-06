﻿using System;
using System.Drawing;
using Raspicam.Net.Config;

namespace Raspicam.Net.Mmal.Ports
{
    interface IMmalPortConfig
    {
        MmalEncoding EncodingType { get; }
        MmalEncoding PixelFormat { get; }
        int Width { get; }
        int Height { get; }
        double Framerate { get; }
        int Quality { get; }
        int Bitrate { get; }
        bool ZeroCopy { get; }
        DateTime? Timeout { get; }
        int BufferNum { get; }
        int BufferSize { get; }
        Rectangle? Crop { get; }
        Split Split { get; }
        bool StoreMotionVectors { get; }
    }
}
