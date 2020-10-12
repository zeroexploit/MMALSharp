﻿using MMALSharp.Common;

namespace MMALSharp.Processors
{
    /// <summary>
    /// A context providing a means to apply image processing. 
    /// </summary>
    public class FrameProcessingContext : IFrameProcessingContext
    {
        private ImageContext _context;

        /// <summary>
        /// Creates a new instance of <see cref="FrameProcessingContext"/>.
        /// </summary>
        /// <param name="context">Metadata for the image frame.</param>
        public FrameProcessingContext(ImageContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public IFrameProcessingContext Apply(IFrameProcessor processor)
        {
            processor.Apply(_context);
            
            return this;
        }
    }
}
