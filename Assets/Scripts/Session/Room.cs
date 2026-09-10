using System.Collections.Generic;
using Core;
using VTT.Unity;

namespace Session
{
    public class Room
    {
        public string Id { get; private set; }
        
        public string Name { get; private set; }
        
        public BoardGrid Grid { get; private set; }
        
        public MapGenerator MapGenerator { get; private set; }
        
        public List<Token> Tokens { get; private set; }

        public Room(string id, string name, BoardGrid boardGrid, MapGenerator mapGenerator)
        {
            Id = id;
            Name = name;
            Grid = boardGrid;
            MapGenerator = mapGenerator;
            
            Tokens = new List<Token>();
        }
    }
}