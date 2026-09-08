namespace Core
{
    public class Token
    {
        public string Name {get; private set;}
        public GridCoordinate Coordinates {get; set;}
        
        public int MaxMovement {get; private set;}
        public int RemainingMovement {get; private set;}
        
        public string Id { get; set; }
        public Faction Faction { get; set; }

        public Token(string name, GridCoordinate coordinates, Faction faction, int movement)
        {
            Name = name;
            this.Coordinates = coordinates;
            Faction = faction;
            MaxMovement = movement;
        }

        public void MoveTo(GridCoordinate coordinate, int movementCost)
        {
            Coordinates = coordinate;
            RemainingMovement = movementCost;
        }

        public void ResetMovement()
        {
            RemainingMovement = MaxMovement;
        }
        
    }
}