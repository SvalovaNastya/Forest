using System;

namespace ForestSolver
{
    public class ForestKeeper
    {
        public int Hp;
        public Point Position;
        readonly public string Name;
        readonly public int Id;
        readonly public Point Destination;

        public ForestKeeper(string name, Point position, Point destination, int hp, int id)
        {
            Position = position;
            Destination = destination;
            Hp = hp;
            Name = name;
            Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ForestKeeper))
                throw new InvalidCastException("obj is not ForestKeeper");
            return Id.Equals(((ForestKeeper)obj).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
