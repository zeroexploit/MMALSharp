﻿using System;
using System.Threading.Tasks;
using MMALSharp.Common;
using MMALSharp.Components;
using MMALSharp.Components.EncoderComponents;
using MMALSharp.Ports;
using MMALSharp.Processing.Handlers;

namespace MMALSharp.Demo
{
    public class ImageOps : OpsBase
    {
        public override void Operations()
        {
            Console.WriteLine("\nPicture Operations:");
            
            Console.WriteLine("1.    Take Picture");
            Console.WriteLine("2.    IsRaw Capture");
            Console.WriteLine("3.    Resize Component");

            var key = Console.ReadKey();
            var formats = this.ParsePixelFormat();

            switch (key.KeyChar)
            {
                case '1':
                    this.TakePictureOperations(formats.Item1, formats.Item2);
                    break;
                case '2':
                    this.TakeRawOperations();
                    break;
                case '3':
                    this.TakeResizeOperations(formats.Item1, formats.Item2);
                    break;
            }
            
            Program.OperationsHandler();
        }
        
        private void TakePictureOperations(MmalEncoding encoding, MmalEncoding pixelFormat)
        {
            Console.WriteLine("\nPlease enter a file extension.");
            var extension = Console.ReadLine();
            this.TakePictureManual(extension, encoding, pixelFormat).GetAwaiter().GetResult();
        }

        private void TakeRawOperations()
        {
            Console.WriteLine("\nPlease enter a file extension.");
            var extension = Console.ReadLine();
            this.TakeRawSensor(extension).GetAwaiter().GetResult();
        }
        
        private void TakeResizeOperations(MmalEncoding encoding, MmalEncoding pixelFormat)
        {
            Console.WriteLine("\nPlease enter a file extension.");
            var extension = Console.ReadLine();
            
            Console.WriteLine("\nEnter the width of the resized image.");
            var width = Console.ReadLine();
            
            Console.WriteLine("\nEnter the height of the resized image.");
            var height = Console.ReadLine();

            int intWidth = 0, intHeight = 0;

            if (!int.TryParse(width, out intWidth) || !int.TryParse(height, out intHeight))
            {
                Console.WriteLine("Invalid values entered, please try again.");
                this.TakeResizeOperations(encoding, pixelFormat);
            }
            
            this.ResizePicture(extension, encoding, pixelFormat, intWidth, intHeight).GetAwaiter().GetResult();
        }
        
        private async Task TakePictureManual(string extension, MmalEncoding encoding, MmalEncoding pixelFormat)
        {
            using (var imgCaptureHandler = new ImageStreamCaptureHandler("/home/pi/images/", extension))
            using (var imgEncoder = new MmalImageEncoder())
            using (var nullSink = new MmalNullSinkComponent())
            {
                this.Cam.ConfigureCameraSettings();
                await Task.Delay(2000);

                var encoderConfig = new MmalPortConfig(encoding, pixelFormat, quality: 90);

                // Create our component pipeline.         
                imgEncoder.ConfigureOutputPort(encoderConfig, imgCaptureHandler);
                
                this.Cam.Camera.StillPort.ConnectTo(imgEncoder);                    
                this.Cam.Camera.PreviewPort.ConnectTo(nullSink);
        
                await this.Cam.ProcessAsync(this.Cam.Camera.StillPort);
            }
        }

        private async Task TakeRawSensor(string extension)
        {
            using (var imgCaptureHandler = new ImageStreamCaptureHandler("/home/pi/images/tests", extension))
            {
                await this.Cam.TakeRawPicture(imgCaptureHandler);
            }
        }
        
        private async Task ResizePicture(string extension, MmalEncoding encoding, MmalEncoding pixelFormat, int width, int height)
        {
            using (var imgCaptureHandler = new ImageStreamCaptureHandler("/home/pi/images/", extension))
            using (var resizer = new MmalResizerComponent())
            using (var imgEncoder = new MmalImageEncoder())
            using (var nullSink = new MmalNullSinkComponent())
            {
                this.Cam.ConfigureCameraSettings();

                await Task.Delay(2000);
                
                var resizerConfig = new MmalPortConfig(pixelFormat, pixelFormat, width: width, height: height);
                var encoderConfig = new MmalPortConfig(encoding, pixelFormat, quality: 90);

                // Create our component pipeline.         
                resizer.ConfigureInputPort(new MmalPortConfig(MmalCameraConfig.Encoding, MmalCameraConfig.EncodingSubFormat), this.Cam.Camera.StillPort, null);
                resizer.ConfigureOutputPort(resizerConfig, null);
                imgEncoder.ConfigureOutputPort(encoderConfig, imgCaptureHandler);
                    
                this.Cam.Camera.StillPort.ConnectTo(resizer);
                resizer.Outputs[0].ConnectTo(imgEncoder);
                this.Cam.Camera.PreviewPort.ConnectTo(nullSink);
                
                await this.Cam.ProcessAsync(this.Cam.Camera.StillPort);
            }
        }
        
        
    }
}