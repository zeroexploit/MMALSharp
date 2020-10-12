﻿using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MMALSharp.Common.Utility;
using MMALSharp.Native;
using static MMALSharp.MmalNativeExceptionHelper;

namespace MMALSharp.Components
{
    public unsafe class MmalCameraInfoComponent : MmalComponentBase, ICameraInfoComponent
    {
        public string SensorName { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }

        public MmalCameraInfoComponent() : base(MmalParameters.MmalComponentDefaultCameraInfo)
        {
            SensorName = "OV5647";
            MaxWidth = 2592;
            MaxHeight = 1944;

            var ptr1 = Marshal.AllocHGlobal(Marshal.SizeOf<MMAL_PARAMETER_CAMERA_INFO_T>());
            var str1 = (MMAL_PARAMETER_HEADER_T*)ptr1;

            str1->Id = MmalParametersCamera.MmalParameterCameraInfo;

            // Deliberately undersize to check if running on older firmware.
            str1->Size = Marshal.SizeOf<MMAL_PARAMETER_CAMERA_INFO_T>();

            try
            {
                // If succeeds, keep OV5647 defaults.
                MmalCheck(MmalPort.mmal_port_parameter_get(Control.Ptr, str1), string.Empty);
            }
            catch
            {
                Marshal.FreeHGlobal(ptr1);

                // Running on newer firmware - default to first camera found.
                var ptr2 = Marshal.AllocHGlobal(Marshal.SizeOf<MMAL_PARAMETER_CAMERA_INFO_V2_T>());
                var str2 = (MMAL_PARAMETER_HEADER_T*)ptr2;

                str2->Id = MmalParametersCamera.MmalParameterCameraInfo;
                str2->Size = Marshal.SizeOf<MMAL_PARAMETER_CAMERA_INFO_V2_T>();

                try
                {
                    MmalCheck(MmalPort.mmal_port_parameter_get(Control.Ptr, str2),
                        "Unable to get camera info for newer firmware.");

                    var p = (IntPtr)str2;

                    var s = Marshal.PtrToStructure<MMAL_PARAMETER_CAMERA_INFO_V2_T>(p);

                    if (s.Cameras != null && s.Cameras.Length > 0)
                    {
                        SensorName = s.Cameras[0].CameraName;
                        MaxHeight = s.Cameras[0].MaxHeight;
                        MaxWidth = s.Cameras[0].MaxWidth;
                    }
                }
                catch
                {
                    // Something went wrong, continue with OV5647 defaults.
                    MmalLog.Logger.LogWarning("Could not determine firmware version. Continuing with OV5647 defaults");
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr2);
                }
            }
        }

        public override void PrintComponent()
        {
            MmalLog.Logger.LogInformation($"Component: Camera info");
        }
    }
}