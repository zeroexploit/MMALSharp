﻿namespace MMALSharp.Components.EncoderComponents
{
    abstract class MmalEncoderBase : MmalDownstreamHandlerComponent, IEncoder
    {
        protected MmalEncoderBase(string encoderName) : base(encoderName) { }
    }
}
