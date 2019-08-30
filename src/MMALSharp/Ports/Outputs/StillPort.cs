﻿// <copyright file="StillPort.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using System;
using MMALSharp.Common.Utility;
using MMALSharp.Components;
using MMALSharp.Native;

namespace MMALSharp.Ports.Outputs
{
    /// <summary>
    /// Represents a still image encoder/decoder port.
    /// </summary>
    public unsafe class StillPort : OutputPort, IStillPort
    {
        /// <inheritdoc />
        public override Resolution Resolution
        {
            get => new Resolution(this.Width, this.Height);
            internal set
            {
                if (value.Width == 0 || value.Height == 0)
                {
                    this.Width = MMALCameraConfig.StillResolution.Pad().Width;
                    this.Height = MMALCameraConfig.StillResolution.Pad().Height;
                }
                else
                {
                    this.Width = value.Pad().Width;
                    this.Height = value.Pad().Height;
                }
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="StillPort"/>. 
        /// </summary>
        /// <param name="ptr">The native pointer.</param>
        /// <param name="comp">The component this port is associated with.</param>
        /// <param name="type">The type of port.</param>
        /// <param name="guid">Managed unique identifier for this component.</param>
        public StillPort(IntPtr ptr, IComponent comp, PortType type, Guid guid)
            : base(ptr, comp, type, guid)
        {
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="StillPort"/>.
        /// </summary>
        /// <param name="copyFrom">The port to copy data from.</param>
        public StillPort(IPort copyFrom)
            : base((IntPtr)copyFrom.Ptr, copyFrom.ComponentReference, copyFrom.PortType, copyFrom.Guid)
        {
        }

        internal override void NativeOutputPortCallback(MMAL_PORT_T* port, MMAL_BUFFER_HEADER_T* buffer)
        {
            if (MMALCameraConfig.Debug)
            {
                MMALLog.Logger.Debug($"In native {nameof(StillPort)} output callback");
            }

            base.NativeOutputPortCallback(port, buffer);
        }
    }    
}
