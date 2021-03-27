using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

using Desktop_Actor.Configuration;

using Newtonsoft.Json;

namespace Desktop_Actor
{
    public class Animator
    {
        private GameObject gameObject;
        private Animation anims;
        private string baseAnimPath;
        private AnimationDetail currAnimDetail;

        public DateTime LastTime;
        public DateTime CurrentTime;

        public Animator(GameObject gameObject)
        {
            this.gameObject = gameObject;

            // Save anim json data and deserialize an Animation class.
            string jsonData = File.ReadAllText(Path.Combine("Data", gameObject.Name, "Animation.json"));
            this.baseAnimPath = Path.Combine("Data", gameObject.Name, "Frames");
            anims = new Animation(JsonConvert.DeserializeObject<AnimationConfig>(jsonData));


            string path = Path.Combine(this.baseAnimPath, anims[AnimationName.Idle].Frames[0]);
            var img = FromFileImage(path);
            gameObject.Dimensions.Width = img.Width * gameObject.Scale;
            gameObject.Dimensions.Height = img.Height * gameObject.Scale;
        }

        // Render target image.
        public void RenderActorFrame(Graphics gfx)
        {
            UpdateAnimState();
            string path = Path.Combine(this.baseAnimPath, currAnimDetail.CurrentFrame);
            var img = FromFileImage(path);
            gameObject.Dimensions.Width = img.Width * gameObject.Scale;
            gameObject.Dimensions.Height = img.Height * gameObject.Scale;

            gfx.SmoothingMode = SmoothingMode.AntiAlias;
            gfx.CompositingQuality = CompositingQuality.HighQuality;

            gfx.DrawImage(img, gameObject.Position.X, gameObject.Position.Y, img.Width * gameObject.Scale, img.Height * gameObject.Scale);
        }

        private void UpdateAnimState()
        {
            currAnimDetail = anims[AnimationName.Idle];
            PointF velocity = gameObject.GetVelocity();

            if (velocity.Y < 0) // UP
            {
                currAnimDetail = anims[AnimationName.Idle];
            }
            else if (velocity.Y > 0) // Down.
            {
                currAnimDetail = anims[AnimationName.Air_vertical];
            }

            if (velocity.X < 0) // Left.
            {
                currAnimDetail = anims[AnimationName.Carry_left];
            }
            else if (velocity.X > 0) // Right.
            {
                currAnimDetail = anims[AnimationName.Carry_right];
            }

            CurrentTime = DateTime.Now;

            if ((CurrentTime - LastTime).TotalSeconds > currAnimDetail.FrameLength)
            {
                currAnimDetail.NextFrame();
                LastTime = CurrentTime;
            }
        }

        // Return target image.
        private Image FromFileImage(string filePath)
        {
            return Image.FromFile(filePath);
        }
    }
}