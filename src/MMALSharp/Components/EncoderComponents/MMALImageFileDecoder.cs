﻿using System;
using System.Threading.Tasks;
using MMALSharp.Common.Handlers;
using MMALSharp.Native;
using MMALSharp.Ports;
using System.Text;

namespace MMALSharp.Components
{
    public class MMALImageFileDecoder : MMALImageDecoder, IMMALConvert
    {        
        public MMALImageFileDecoder(TransformStreamCaptureHandler handler)
            : base(handler)
        {
        }

        public static MMALQueueImpl WorkingQueue { get; set; }

        public override unsafe void ConfigureInputPort(MMALEncoding encodingType, MMALEncoding pixelFormat, int width, int height, bool zeroCopy = false)
        {
            this.InitialiseInputPort(0);

            if (encodingType != null)
            {
                this.Inputs[0].Ptr->Format->encoding = encodingType.EncodingVal;
            }
                        
            this.Inputs[0].Ptr->Format->type = MMALFormat.MMAL_ES_TYPE_T.MMAL_ES_TYPE_VIDEO;
            this.Inputs[0].Ptr->Format->es->video.height = 0;
            this.Inputs[0].Ptr->Format->es->video.width = 0;
            this.Inputs[0].Ptr->Format->es->video.frameRate = new MMAL_RATIONAL_T(0, 1);
            this.Inputs[0].Ptr->Format->es->video.par = new MMAL_RATIONAL_T(1, 1);
            
            this.Inputs[0].EncodingType = encodingType;

            this.Inputs[0].Commit();
            
            this.Inputs[0].Ptr->BufferNum = Math.Max(this.Inputs[0].Ptr->BufferNumRecommended, this.Inputs[0].Ptr->BufferNumMin);
            this.Inputs[0].Ptr->BufferSize = Math.Max(this.Inputs[0].Ptr->BufferSizeRecommended, this.Inputs[0].Ptr->BufferSizeMin);            
        }

        public override unsafe void ConfigureOutputPort(int outputPort, MMALEncoding encodingType, MMALEncoding pixelFormat, int quality, int bitrate = 0, bool zeroCopy = false)
        {
            this.InitialiseOutputPort(outputPort);
            this.ProcessingPorts.Add(outputPort);
                       
            if (encodingType != null)
            {
                this.Outputs[outputPort].Ptr->Format->encoding = encodingType.EncodingVal;
            }

            if (zeroCopy)
            {
                this.Outputs[outputPort].ZeroCopy = true;
                this.Outputs[outputPort].SetParameter(MMALParametersCommon.MMAL_PARAMETER_ZERO_COPY, true);
            }

            this.Outputs[outputPort].Commit();
                        
            this.Outputs[outputPort].EncodingType = encodingType;

            this.Outputs[outputPort].Ptr->BufferNum = Math.Max(this.Outputs[outputPort].Ptr->BufferNumRecommended, this.Outputs[outputPort].Ptr->BufferNumMin);
            this.Outputs[outputPort].Ptr->BufferSize = Math.Max(this.Outputs[outputPort].Ptr->BufferSizeRecommended, this.Outputs[outputPort].Ptr->BufferSizeMin);                        
        }

        internal unsafe void ConfigureOutputPortWithoutInit(int outputPort, MMALEncoding encodingType)
        {
            if (encodingType != null)
            {
                this.Outputs[outputPort].Ptr->Format->encoding = encodingType.EncodingVal;
            }
                                    
            this.Outputs[outputPort].EncodingType = encodingType;

            this.Outputs[outputPort].Ptr->BufferNum = 2;
            this.Outputs[outputPort].Ptr->BufferSize = this.Outputs[outputPort].Ptr->BufferSizeRecommended;

            MMALLog.Logger.Info($"New buffer number {this.Outputs[outputPort].Ptr->BufferNum}");
            MMALLog.Logger.Info($"New buffer size {this.Outputs[outputPort].Ptr->BufferSize}");

            this.Outputs[outputPort].Commit();
        }

        internal void LogFormat(MMALEventFormat format, MMALPortImpl port)
        {
            StringBuilder sb = new StringBuilder();

            if (port != null)
            {
                switch (port.PortType)
                {
                    case PortType.Input:
                        sb.Append("Port Type: Input");
                        break;
                    case PortType.Output:
                        sb.Append("Port Type: Output");
                        break;
                    case PortType.Control:
                        sb.Append("Port Type: Control");
                        break;
                    default:
                        break;
                }
            }
                        
            sb.Append($"FourCC: {format.FourCC}");
            sb.Append($"Width: {format.Width}");
            sb.Append($"Height: {format.Height}");
            sb.Append($"Crop: {format.CropX}, {format.CropY}, {format.CropWidth}, {format.CropHeight}");
            sb.Append($"Pixel aspect ratio: {format.ParNum}, {format.ParDen}. Frame rate: {format.FramerateNum}, {format.FramerateDen}");
            
            if (port != null)
            {
                sb.Append($"Port info: Buffers num: {port.BufferNum}(opt {port.BufferNumRecommended}, min {port.BufferNumMin}). Size: {port.BufferSize} (opt {port.BufferSizeRecommended}, min {port.BufferSizeMin}). Alignment: {port.BufferAlignmentMin}");
            }
            
            MMALLog.Logger.Info(sb.ToString());
        }

        public async Task WaitForTriggers(int outputPort = 0)
        {
            MMALLog.Logger.Debug("Waiting for trigger signal");
            // Wait until the process is complete.

            while (this.Inputs[0].Trigger.CurrentCount > 0 && this.Outputs[outputPort].Trigger.CurrentCount > 0)
            {
                MMALLog.Logger.Info("Awaiting...");
                await Task.Delay(2000);
                break;
            }

            MMALLog.Logger.Debug("Setting countdown events");
            this.Inputs[0].Trigger = new Nito.AsyncEx.AsyncCountdownEvent(1);
            this.Outputs[outputPort].Trigger = new Nito.AsyncEx.AsyncCountdownEvent(1);
        }

        internal void GetAndSendInputBuffer()
        {
            //Get buffer from input port pool                
            MMALBufferImpl inputBuffer;
            lock (MMALPortBase.InputLock)
            {                
                inputBuffer = this.Inputs[0].BufferPool.Queue.GetBuffer();

                if (inputBuffer != null)
                {
                    // Populate the new input buffer with user provided image data.
                    var result = this.ManagedInputCallback(inputBuffer, this.Inputs[0]);
                    inputBuffer.ReadIntoBuffer(result.BufferFeed, result.DataLength, result.EOF);

                    this.Inputs[0].SendBuffer(inputBuffer);
                }
            }
        }

        internal void GetAndSendOutputBuffer(int outputPort = 0)
        {
            while (true)
            {
                lock (MMALPortBase.OutputLock)
                {
                    var tempBuf2 = this.Outputs[outputPort].BufferPool.Queue.GetBuffer();

                    if (tempBuf2 != null)
                    {
                        // Send empty buffers to the output port of the decoder                                          
                        this.Outputs[outputPort].SendBuffer(tempBuf2);
                    }
                    else
                    {
                        MMALLog.Logger.Debug("GetAndSendOutputBuffer: Buffer null.");
                        break;
                    }
                }
            }
        }

        internal void ProcessFormatChangedEvent(MMALBufferImpl buffer, int outputPort = 0)
        {            
            MMALLog.Logger.Debug("Received MMAL_EVENT_FORMAT_CHANGED event");

            var ev = MMALEventFormat.GetEventFormat(buffer);

            MMALLog.Logger.Debug("-- Event format changed from -- ");
            this.LogFormat(new MMALEventFormat(this.Outputs[outputPort].Format), this.Outputs[outputPort]);

            MMALLog.Logger.Debug("-- To -- ");
            this.LogFormat(ev, null);

            // Port format changed
            this.ManagedOutputCallback(buffer, this.Outputs[outputPort]);

            lock (MMALPortBase.OutputLock)
            {                
                buffer.Release();
            }
                        
            this.Outputs[outputPort].DisablePort();

            while (this.Outputs[outputPort].BufferPool.Queue.QueueLength() < this.Outputs[outputPort].BufferPool.HeadersNum)
            {
                MMALLog.Logger.Debug("Queue length less than buffer pool num");
                lock (MMALPortBase.OutputLock)
                {
                    MMALLog.Logger.Debug("Getting buffer via Queue.Wait");
                    var tempBuf = WorkingQueue.Wait();                                        
                    tempBuf.Release();
                }
            }
                        
            this.Outputs[outputPort].BufferPool.Dispose();
                        
            this.Outputs[outputPort].FullCopy(ev);
                        
            this.ConfigureOutputPortWithoutInit(0, this.Outputs[outputPort].EncodingType);
                        
            this.Outputs[outputPort].EnablePort(this.ManagedOutputCallback, false);            
        }
        
        /// <summary>
        /// Encodes/decodes user provided image data
        /// </summary>
        /// <param name="outputPort">The output port to begin processing on. Usually will be 0.</param>
        /// <returns>An awaitable task</returns>
        public virtual async Task Convert(int outputPort = 0)
        {
            MMALLog.Logger.Debug("Beginning Image decode from filestream. Please note, this process may take some time depending on the size of the input image.");

            this.Inputs[0].Trigger = new Nito.AsyncEx.AsyncCountdownEvent(1);
            this.Outputs[outputPort].Trigger = new Nito.AsyncEx.AsyncCountdownEvent(1);

            // Enable control, input and output ports. Input & Output ports should have been pre-configured by user prior to this point.
            this.Start(this.Control, new Action<MMALBufferImpl, MMALPortBase>(this.ManagedControlCallback));
            this.Start(this.Inputs[0], this.ManagedInputCallback);
            this.Start(this.Outputs[outputPort], new Action<MMALBufferImpl, MMALPortBase>(this.ManagedOutputCallback));
                        
            this.EnableComponent();

            WorkingQueue = MMALQueueImpl.Create();

            var eosReceived = false;

            while (!eosReceived)
            {
                await this.WaitForTriggers();

                this.GetAndSendInputBuffer();
      
                MMALLog.Logger.Debug("Getting processed output pool buffer");
                while (true)
                {
                    MMALBufferImpl buffer;
                    lock (MMALPortBase.OutputLock)
                    {
                        buffer = WorkingQueue.GetBuffer();
                    }
                                        
                    if (buffer != null)
                    {                                                
                        eosReceived = ((int)buffer.Flags & (int)MMALBufferProperties.MMAL_BUFFER_HEADER_FLAG_EOS) == (int)MMALBufferProperties.MMAL_BUFFER_HEADER_FLAG_EOS;

                        if (buffer.Cmd > 0)
                        {
                            if (buffer.Cmd == MMALEvents.MMAL_EVENT_FORMAT_CHANGED)
                            {
                                this.ProcessFormatChangedEvent(buffer);                                
                            }
                            else
                            {
                                lock (MMALPortBase.OutputLock)
                                {
                                    buffer.Release();
                                }
                            }
                            continue;
                        }
                        else
                        {
                            if (buffer.Length > 0)
                            {
                                this.ManagedOutputCallback(buffer, this.Outputs[outputPort]);
                            }
                            else
                            {
                                MMALLog.Logger.Debug("Buffer length empty.");
                            }

                            // Ensure we release the buffer before any signalling or we will cause a memory leak due to there still being a reference count on the buffer.                    
                            lock (MMALPortBase.OutputLock)
                            {
                                buffer.Release();
                            }
                        }                        
                    }
                    else
                    {
                        MMALLog.Logger.Debug("Buffer null.");
                        break;
                    }
                }

                this.GetAndSendOutputBuffer();                                     
            }

            MMALLog.Logger.Info("Received EOS. Exiting.");

            this.DisableComponent();
            this.CleanPortPools();
            WorkingQueue.Dispose();
        }

        internal override unsafe void InitialiseInputPort(int inputPort)
        {
            this.Inputs[inputPort] = new MMALStillDecodeConvertPort(&(*this.Ptr->Input[inputPort]), this, PortType.Input);
        }

        internal override unsafe void InitialiseOutputPort(int outputPort)
        {
            this.Outputs[outputPort] = new MMALStillDecodeConvertPort(&(*this.Ptr->Output[outputPort]), this, PortType.Output);
        }
    }
}
