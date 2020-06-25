using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gamemaster.Models
{
    public class Scene
    {
        public Dictionary<string, Unit> Units { get; set; } = new Dictionary<string, Unit>();

        internal void AddUnit(string id, Unit unit)
        {
            Units.Add(id, unit);
        }
        internal void Drag(string id, int x, int y)
        {
            Units[id].X = x;
            Units[id].Y = y;
        }
        internal void Move(string id, Direction d)
        {
            switch (d)
            {
                case Direction.North:
                    Units[id].Y -= 1;
                    break;
                case Direction.East:
                    Units[id].X += 1;
                    break;
                case Direction.South:
                    Units[id].Y += 1;
                    break;
                case Direction.West:
                    Units[id].X -= 1;
                    break;
            }
        }

        internal void RemoveUnit(string id)
        {
            Units.Remove(id);
        }
    }
}
