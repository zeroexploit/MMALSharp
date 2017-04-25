﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MMALSharp.Tests
{
    public class MMALFixture : IDisposable
    {
        public MMALCamera MMALCamera = MMALCamera.Instance;

        public void Dispose()
        {
            this.MMALCamera.Cleanup();
        }
    }

    [CollectionDefinition("MMALCollection")]
    public class MMALCollection : ICollectionFixture<MMALFixture>
    {

    }
}