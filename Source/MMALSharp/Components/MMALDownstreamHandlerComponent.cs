﻿namespace MMALSharp.Components
{
    abstract class MmalDownstreamHandlerComponent : MmalDownstreamComponent, IDownstreamHandlerComponent
    {
        protected MmalDownstreamHandlerComponent(string name) : base(name) { }
    }
}
