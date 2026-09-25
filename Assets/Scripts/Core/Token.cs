namespace Core
{
    public class Token
    {
        public string Id { get; private set; }
        
        public string Name {get; private set;}
        
        public GridCoordinate Coordinates {get; set;}
        
        public int MaxMovement {get; private set;}
        
        public Faction Faction { get; set; }

        public Token(string id, string name, GridCoordinate coordinates, Faction faction, int movement)
        {
            Id = id;
            Name = name;
            this.Coordinates = coordinates;
            Faction = faction;
            MaxMovement = movement;
        }

        public void MoveTo(GridCoordinate coordinate, int movementCost)
        {
            Coordinates = coordinate;
        }
        
    }
}