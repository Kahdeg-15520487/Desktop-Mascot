using Humper;
using Humper.Base;
using Humper.Responses;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_Actor.CollisionResponses
{
    class ConveyorCollisionResponse : ICollisionResponse
    {
        public ConveyorCollisionResponse(ICollision collision, float speed = 20f)
        {
            var slide = new Vector2(speed, 0);

            this.Destination = new RectangleF(collision.Hit.Position + slide, collision.Goal.Size);
        }
        public RectangleF Destination { get; private set; }
    }
}
