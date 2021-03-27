using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_Actor.Configuration
{
    public class AnimationConfig
    {
        public AnimationConfigDetail[] Details { get; set; }
    }

    public class AnimationConfigDetail
    {
        public string Name { get; set; }
        public float FrameLength { get; set; }
        public string[] Frames { get; set; }
    }
}
