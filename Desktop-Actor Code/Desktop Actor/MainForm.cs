using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Humper;

// This is the code for your desktop app.
// Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.

namespace Desktop_Actor
{
    public partial class MainForm : Form
    {
        private GameObject actor;
        private GameObject spawner;
        private Rectangle gameObjectPlayArea;
        private World world;

        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Focus();
            BringToFront();
            TabStop = false;
            TopMost = true;
            KeyPreview = true;  // Prioritize parent key press over child.
            BackColor = Color.LimeGreen;
            TransparencyKey = Color.LimeGreen;
            ShowInTaskbar = false;
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;

            // Default boundary.
            gameObjectPlayArea = new Rectangle(0, 0, Screen.FromControl(this).Bounds.Width, Screen.FromControl(this).Bounds.Height);
            world = new World(gameObjectPlayArea.Width, gameObjectPlayArea.Height);

            // Player char and animator component.
            actor = new GameObject(this, gameObjectPlayArea, "konluludoll", 1200, 0.2f);
            actor.Collision = world.Create(actor.Position.X, actor.Position.Y, actor.Dimensions.Width, actor.Dimensions.Height);
            actor.Collision.Data = actor;

            // Spawner
            spawner = new GameObject(this, gameObjectPlayArea, "conveyor", 0, 0.2f);
            spawner.Collision = world.Create(spawner.Position.X, spawner.Position.Y, spawner.Dimensions.Width, spawner.Dimensions.Height);
            spawner.Collision.Data = spawner;
        }

        protected override void OnPaint(PaintEventArgs eventArgs)
        {
            base.OnPaint(eventArgs);

            // Render actor.
            var gfx = eventArgs.Graphics;
            this.actor.Animator.RenderActorFrame(gfx);
            this.spawner.Animator.RenderActorFrame(gfx);
            gfx.ResetTransform();

            // Update movement position relative to fps.
            var fps = this.CalculateFPS();
            this.actor.UpdatePositions(fps);
            this.spawner.UpdatePositions(fps);

            Invalidate(); // Force control to be redrawn.
        }

        public DateTime PrevFrameTime;
        public DateTime FrameTime;

        public double CalculateFPS()
        {
            // Default starting prev to earlier time for fps.
            if (PrevFrameTime == FrameTime)
            {
                PrevFrameTime = DateTime.Now.AddMilliseconds(-2);
            }
            else
            {
                PrevFrameTime = FrameTime;
            }

            FrameTime = DateTime.Now;
            return (FrameTime - PrevFrameTime).TotalMilliseconds / 1000;
        }
    }
}