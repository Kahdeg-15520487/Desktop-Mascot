using Desktop_Actor.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_Actor
{
    class Animation : Dictionary<string, AnimationDetail>
    {
        public Animation(AnimationConfig config) : base()
        {
            foreach (AnimationConfigDetail detail in config.Details)
            {
                this.Add(detail.Name, new AnimationDetail(detail));
            }
        }

        public AnimationDetail this[AnimationName index] {
            get => this[index.ToString()];
        }
        public AnimationDetail GetAnimationDetail(AnimationName animationName)
        {
            return this[animationName.ToString()];
        }
    }

    class AnimationDetail
    {

        public AnimationDetail(AnimationConfigDetail detail)
        {
            this.Name = detail.Name;
            this.FrameLength = detail.FrameLength;
            this.Frames = detail.Frames;
            this.CurrentFrameIndex = 0;
        }

        public string Name { get; }
        public float FrameLength { get; }
        public string[] Frames { get; }
        public int CurrentFrameIndex { get; private set; }
        public string CurrentFrame => Frames[CurrentFrameIndex];

        public int NextFrame()
        {
            CurrentFrameIndex++;
            if (CurrentFrameIndex >= Frames.Length)
            {
                CurrentFrameIndex = 0;
            }

            return CurrentFrameIndex;
        }
    }

    enum AnimationName
    {

        Idle,
        Carry_left,
        Carry_right,
        Air_vertical,
        Air_horizontal,
        Walk_left,
    }
}
