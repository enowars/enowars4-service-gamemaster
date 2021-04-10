namespace GamemasterChecker.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Scene
    {
        public Dictionary<string, Unit> Units { get; set; } = new Dictionary<string, Unit>();

        internal void AddUnit(string id, Unit unit)
        {
            this.Units.Add(id, unit);
        }

        internal void Drag(string id, int x, int y)
        {
            this.Units[id].X = x;
            this.Units[id].Y = y;
        }

        internal void Move(string id, Direction d)
        {
            switch (d)
            {
                case Direction.North:
                    this.Units[id].Y -= 1;
                    break;
                case Direction.East:
                    this.Units[id].X += 1;
                    break;
                case Direction.South:
                    this.Units[id].Y += 1;
                    break;
                case Direction.West:
                    this.Units[id].X -= 1;
                    break;
            }
        }

        internal void RemoveUnit(string id)
        {
            this.Units.Remove(id);
        }
    }
}
