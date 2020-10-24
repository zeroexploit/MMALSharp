﻿using System;
using Microsoft.Extensions.Logging;
using MMALSharp.Components;
using MMALSharp.Config;
using MMALSharp.Extensions;
using MMALSharp.Handlers;
using MMALSharp.Native.Buffer;
using MMALSharp.Native.Parameters;
using MMALSharp.Native.Port;
using MMALSharp.Ports.Inputs;
using MMALSharp.Utility;

namespace MMALSharp.Ports.Outputs
{
    unsafe class StillPort : OutputPort, IStillPort
    {
        public override Resolution Resolution
        {
            get => new Resolution(Width, Height);
            internal set
            {
                if (value.Width == 0 || value.Height == 0)
                {
                    Width = MmalCameraConfig.Resolution.Pad().Width;
                    Height = MmalCameraConfig.Resolution.Pad().Height;
                }
                else
                {
                    Width = value.Pad().Width;
                    Height = value.Pad().Height;
                }
            }
        }

        public StillPort(IntPtr ptr, IComponent comp, Guid guid) : base(ptr, comp, guid) { }

        public StillPort(IPort copyFrom) : base((IntPtr)copyFrom.Ptr, copyFrom.ComponentReference, copyFrom.Guid) { }

        public override void Configure(IMmalPortConfig config, IInputPort copyFrom, ICaptureHandler handler)
        {
            base.Configure(config, copyFrom, handler);

            if (config != null && config.EncodingType == MmalEncoding.Jpeg)
                this.SetParameter(MmalParametersCamera.MmalParameterJpegQFactor, config.Quality);
        }

        internal override void NativeOutputPortCallback(MmalPortType* port, MmalBufferHeader* buffer)
        {
            if (MmalCameraConfig.Debug)
                MmalLog.Logger.LogDebug($"{Name}: In native {nameof(StillPort)} output callback");

            base.NativeOutputPortCallback(port, buffer);
        }
    }
}
