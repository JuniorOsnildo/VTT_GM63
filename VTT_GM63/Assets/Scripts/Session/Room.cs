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
        
        public MapData MapData { get; private set; }
        
        public List<Token> Tokens { get; private set; }
        
        private int nextTokenId = 1;

        public Room(string id, string name, BoardGrid boardGrid, MapData mapData)
        {
            Id = id;
            Name = name;
            Grid = boardGrid;
            MapData = mapData;
            
            Tokens = new List<Token>();
        }
        
        public Token CreateToken(string name, GridCoordinate coordinate, Faction faction, int maxMovement)
        {
            string tokenId = $"{Id}_token_{nextTokenId}";
            nextTokenId++;

            Token token = new Token(
                tokenId,
                name,
                coordinate,
                faction,
                maxMovement
            );

            Tokens.Add(token);

            Grid.PlaceToken(token, coordinate);

            return token;
        }
        
        public Token GetTokenById(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
                return null;

            return Tokens.Find(
                token => token.Id == tokenId
            );
        }
        
        public void AddToken(Token token)
        {
            if (token == null)
                return;

            Tokens.Add(token);
            Grid.PlaceToken(token, token.Coordinates);

            UpdateNextTokenId(token.Id);
        }
        
        private void UpdateNextTokenId(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
                return;

            string prefix = $"{Id}_token_";

            if (!tokenId.StartsWith(prefix))
                return;

            string numberPart = tokenId.Substring(prefix.Length);

            if (int.TryParse(numberPart, out int tokenNumber))
            {
                if (tokenNumber >= nextTokenId)
                    nextTokenId = tokenNumber + 1;
            }
        }
    }
}