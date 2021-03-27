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
        private AnimationConfig anims;
        private string baseAnimPath;

        public Animator(GameObject gameObject)
        {
            this.gameObject = gameObject;

            // Save anim json data and deserialize an Animation class.
            string jsonData = File.ReadAllText(Path.Combine("Data", gameObject.Name, "Animation.json"));
            this.baseAnimPath = Path.Combine("Data", gameObject.Name, "Frames");
            anims = JsonConvert.DeserializeObject<AnimationConfig>(jsonData);


            string animState = GetAnimState();
            string path = Path.Combine(this.baseAnimPath, animState);
            var img = FromFileImage(path);
            gameObject.Dimensions.Width = img.Width * gameObject.Scale;
            gameObject.Dimensions.Height = img.Height * gameObject.Scale;
        }

        // Render target image.
        public void RenderActorFrame(Graphics gfx)
        {
            string animState = GetAnimState();
            string path = Path.Combine(this.baseAnimPath, animState);
            var img = FromFileImage(path);
            gameObject.Dimensions.Width = img.Width * gameObject.Scale;
            gameObject.Dimensions.Height = img.Height * gameObject.Scale;

            gfx.SmoothingMode = SmoothingMode.AntiAlias;
            gfx.CompositingQuality = CompositingQuality.HighQuality;

            gfx.DrawImage(img, gameObject.Position.X, gameObject.Position.Y, img.Width * gameObject.Scale, img.Height * gameObject.Scale);
        }

        private string GetAnimState()
        {
            string[] animFrames = anims.Idle;
            PointF velocity = gameObject.GetVelocity();

            if (velocity.Y < 0) // UP
            {
                animFrames = anims.Idle;
            }
            else if (velocity.Y > 0) // Down.
            {
                animFrames = anims.Air_vertical;
            }

            if (velocity.X < 0) // Left.
            {
                animFrames = anims.Carry_left;
            }
            else if (velocity.X > 0) // Right.
            {
                animFrames = anims.Carry_right;
            }

            return animFrames[0];
        }

        // Return target image.
        private Image FromFileImage(string filePath)
        {
            return Image.FromFile(filePath);
        }
    }
}