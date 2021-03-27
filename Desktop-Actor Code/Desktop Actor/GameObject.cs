using Desktop_Actor.Configuration;

using Humper;
using Humper.Responses;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Desktop_Actor
{
    public class GameObject
    {
        private readonly RectangleF _playArea;
        public PointF Position;
        public Dimensions Dimensions;
        public float Scale;
        public bool CursorDragging { get; private set; }
        public string Name { get; private set; }
        public Animator Animator { get; private set; }
        public IBox Collision { get; internal set; }
        public float BaseSpeed { get; internal set; }
        public CollisionConfig CollisionConfig { get; private set; }

        public GameObject(Control form, Rectangle playAreaBoundary, string name, float speed, float scale = 1f)
        {
            this._playArea = playAreaBoundary;

            Position = new Point();
            Dimensions = new Dimensions();

            // Subscribe to key press events.
            form.MouseDown += MouseClick;
            form.MouseUp += MouseUp;

            // Starting position.
            this.Scale = scale;
            this.BaseSpeed = speed;

            this.Name = name;
            this.Animator = new Animator(this);


            // load collision json data
            string jsonData = File.ReadAllText(Path.Combine("Data", Name, "Collision.json"));
            CollisionConfig = JsonConvert.DeserializeObject<CollisionConfig>(jsonData);

            ArraySetup(); // Setup arrays for overflow handling.
        }

        /// <summary>
        /// Keeps game object inside of the play area's boundaries.
        /// </summary>
        public void KeepInsidePlayArea()
        {
            // If top of GameObject is higher than top of the boundary.
            if (Position.Y < _playArea.Y)
            {
                Position.Y = _playArea.Y;
            }
            // If bottom of GameObject is lower than bottom of the boundary.
            else if ((Position.Y + Dimensions.Height) > (_playArea.Y + _playArea.Height))
            {
                Position.Y = _playArea.Y + (_playArea.Height - Dimensions.Height);
                ResetVelocity(curPosDif);
            }

            // If left of GameObject is outside of boundary's left side.
            if (Position.X < _playArea.X)
            {
                Position.X = _playArea.X;
            }
            // If right of GameObject is outside of boundary's right side.
            else if ((Position.X + Dimensions.Width) > (_playArea.X + _playArea.Width))
            {
                Position.X = (_playArea.X + _playArea.Width) - Dimensions.Width;
            }
        }

        public bool IsInBound(float mX, float mY)
        {
            float boundX = Position.X + Dimensions.Width;
            float boundY = Position.Y + Dimensions.Height;
            return mX > Position.X && mX < boundX
                && mY > Position.Y && mY < boundY;
        }

        #region Event Methods

        private void MouseClick(object sender, MouseEventArgs e)
        {
            CursorDragging = IsInBound(e.X, e.Y);
            Console.WriteLine("CALL");
        }

        private void MouseUp(object sender, MouseEventArgs e)
        {
            CursorDragging = false;
            Console.WriteLine("RELEASE");
        }

        #endregion Event Methods

        /// <summary>
        /// Returns point of left side at actor center height.
        /// </summary>
        /// <returns></returns>
        public PointF Top()
        {
            return new PointF(Position.X + Dimensions.Width / 2, Position.Y);
        }

        public PointF Bottom()
        {
            return new PointF(Position.X + Dimensions.Width / 2, Position.Y + Dimensions.Height);
        }

        public PointF Left()
        {
            return new PointF(Position.X, Position.Y + Dimensions.Height / 2);
        }

        public PointF Right()
        {
            return new PointF(Position.X + Dimensions.Width, Position.Y + Dimensions.Height / 2);
        }

        #region Movement


        private PointF velocity;     // Calculate velocity based on average distance per sec.
        private int n = 0, d = 6;   // Array overflows at length 5, d is n-1.
        private PointF[] prevPos = new PointF[7];     // Save previous positions to get distance moved.
        private PointF[] curPosDif = new PointF[7];   // Save distances between previous positions.

        private void ArraySetup()
        {
            for (int i = 0; i < prevPos.Length; i++)
            {
                prevPos[i] = this.Position;
                curPosDif[i].X = 0;
                curPosDif[i].Y = 0;
            }
        }

        public void UpdatePositions(double framesPerSecond)
        {
            // Ensure that the motion is moving at a
            float speed = this.BaseSpeed; // X units within 1 second
            float gravity = speed * (float)framesPerSecond;

            // Follow cursor or apply gravity.
            if (this.CursorDragging)
            {
                CursorDragActor();
            }
            else
            {
                Gravity(gravity);
            }

            this.KeepInsidePlayArea(); // Keep gameobject inside boundaries.

            // Update cur Point and calculate dist moved from last pos.
            prevPos[n] = this.Position;
            curPosDif[n].X = prevPos[n].X - prevPos[d].X;
            curPosDif[n].Y = prevPos[n].Y - prevPos[d].Y;

            // Increment so that curPos overflows at array end.
            n = OverflowInt(n);
            d = OverflowInt(d);

            // Calculate avg of total differences between movement in array.
            // Then move gameobject based on that to simulate velocity/physics.
            PhysicsMovement(gravity);
        }

        public PointF GetVelocity()
        {
            return this.SumAvg(curPosDif);
        }

        /// <summary>
        /// Set gameobject position to be centered with cursor.
        /// </summary>
        private void CursorDragActor()
        {
            this.Position.X = (int)(Cursor.Position.X - (this.Dimensions.Width / 2));
            this.Position.Y = (int)(Cursor.Position.Y - (this.Dimensions.Height / 2));
        }

        /// <summary>
        /// Apply gravitational force and logic.
        /// </summary>
        /// <param name="moveDistPerSecond">Allowed fall distance per second.</param>
        public void Gravity(float moveDistPerSecond)
        {
            this.Position.Y += moveDistPerSecond;
        }

        /// <summary>
        /// Return incremented integar with overflow.
        /// </summary>
        /// <param name="i">Integar value to increment or overflow.</param>
        /// <returns></returns>
        private int OverflowInt(int i)
        {
            i++;
            if (i < 0)
                return prevPos.Length - 1;
            if (i > prevPos.Length - 1)
                return 0;
            return i;
        }

        /// <summary>
        /// Returns fake velocity based on averaged distance moved per update.
        /// </summary>
        /// <param name="points">Array of distances to average.</param>
        /// <returns></returns>
        private PointF SumAvg(PointF[] points)
        {
            PointF sumAvg = new PointF(0, 0);
            for (int i = 0; i < points.Length - 1; i++)
            {
                sumAvg.X += points[i].X;
                sumAvg.Y += points[i].Y;
            }
            sumAvg.X /= points.Length;
            sumAvg.Y /= points.Length;

            return sumAvg;
        }

        /// <summary>
        /// Returns fake velocity based on averaged distance moved per update.
        /// </summary>
        /// <param name="points">Array of distances to average.</param>
        /// <returns></returns>
        private void ResetVelocity(PointF[] points)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                points[i].X = 0;
                points[i].Y = 0;
            }
        }

        /// <summary>
        /// Apply physics forces on gameobject.
        /// </summary>
        /// <param name="moveDistPerSec">Allowed movement distance per second.</param>
        public void PhysicsMovement(float moveDistPerSec)
        {
            velocity = SumAvg(curPosDif);
            var newX = this.Position.X + velocity.X;
            var newY = this.Position.Y + velocity.Y;
            var moveResult = this.Collision.Move(newX, newY, (collision) =>
            {
                var other = collision.Other.Data as GameObject;
                if (other?.CollisionConfig.Type == ObjectType.Conveyor)
                {
                    var conveyorSpeed = other.CollisionConfig.ConveyorSpeed;
                    Console.WriteLine(conveyorSpeed);
                    return new Desktop_Actor.CollisionResponses.ConveyorCollisionResponse(collision, conveyorSpeed);
                }
                else
                {
                    return new Humper.SlideResponse(collision);
                }
            });

            if (moveResult.HasCollided)
            {
                ResetVelocity(curPosDif);
            }

            this.Position.X = this.Collision.X;
            this.Position.Y = this.Collision.Y;
        }

        /// <summary>
        /// Clamps speed but limiting maximum movement each update from exceeding maximum.
        /// </summary>
        /// <param name="prevPosition"></param>
        /// <param name="moveDistPerSecond"></param>
        private void ClampSpeed(Point prevPosition, float moveDistPerSecond)
        {
            // Get distance from positions before and after movement during this update.
            var distance = new PointF(prevPosition.X - this.Position.X, prevPosition.Y - this.Position.X);
            var clampSpeedAmount = (int)(moveDistPerSecond * .32f); // Amount to reduce speed by.

            if (distance.X <= 0 || distance.Y <= 0) return;
            // Apply clamp to X axis.
            if (this.Position.X < 0)
                this.Position.X -= clampSpeedAmount;
            else
                this.Position.X += clampSpeedAmount;
            // Apply clamp to Y axis.
            if (this.Position.Y < 0)
                this.Position.Y -= clampSpeedAmount;
            else
                this.Position.Y += clampSpeedAmount;
        }
        #endregion
    }

    public struct Dimensions
    {
        public float Width;
        public float Height;
    }
}