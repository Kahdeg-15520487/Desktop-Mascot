using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_Actor.Configuration
{
    public class CollisionConfig
    {
        public ObjectType Type { get; set; } = ObjectType.Actor;
        public float ConveyorSpeed { get; set; } = 0;
    }

    public enum ObjectType
    {
        Actor,
        Static,
        Conveyor
    }
}
