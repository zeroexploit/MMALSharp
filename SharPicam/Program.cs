﻿using SharPicam.Components;
using SharPicam.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SharPicam.Native.MMALParametersCamera;
using static SharPicam.MMALParameterHelpers;

namespace SharPicam
{
    public unsafe class Program
    {
        public static void Main(string[] args)
        {
            BcmHost.bcm_host_init();

            var camera = new MMALCameraComponent();
            //var encoder = new MMALEncoderComponent();
            var nullSink = new MMALNullSinkComponent();

            var previewPort = camera.Outputs.ElementAt(MMALCameraComponent.MMAL_CAMERA_PREVIEW_PORT);
            var videoPort = camera.Outputs.ElementAt(MMALCameraComponent.MMAL_CAMERA_VIDEO_PORT);
            var stillPort = camera.Outputs.ElementAt(MMALCameraComponent.MMAL_CAMERA_CAPTURE_PORT);

            var nullSinkInputPort = nullSink.Inputs.ElementAt(0);
            var nullSinkConnection = MMALConnectionImpl.CreateConnection(previewPort.Ptr, nullSinkInputPort.Ptr);
           
            camera.Control.SetShutterSpeed(0);
            
            stillPort.EnablePort(camera.CameraBufferCallback);

            var length = 0u;
            while (camera.BufferPool.Queue.QueueLength() == 0)
            {
                Console.WriteLine("Queue empty. Waiting...");
                Thread.Sleep(1000);
            }

            length = camera.BufferPool.Queue.QueueLength();

            Console.WriteLine("Buffer queue length " + length);

            for (int i = 0; i < length; i++)
            {
                var buffer = camera.BufferPool.Queue.GetBuffer();
                stillPort.SendBuffer(buffer.Ptr);
            }

            Console.WriteLine("Attempt capture");
            SetParameter(MMAL_PARAMETER_CAPTURE, 1, stillPort.Ptr);

            Console.ReadLine();

            camera.Control.DisablePort();
            stillPort.DisablePort();
            nullSinkConnection.Dispose();
            camera.Dispose();
            nullSink.Dispose();

            BcmHost.bcm_host_deinit();            
        }
    }
}
