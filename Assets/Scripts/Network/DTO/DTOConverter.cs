using Core;
using Session;
using VTT.Unity;

namespace Network.DTO
{
    public static class DTOConverter
    {
        public static TokenDTO ToDTO(Token token)
        {
            if (token == null)
                return null;

            return new TokenDTO
            {
                Id = token.Id,
                Name = token.Name,

                X = token.Coordinates.X,
                Y = token.Coordinates.Y,

                Faction = (int)token.Faction,

                MaxMovement = token.MaxMovement,
                RemainingMovement = token.RemainingMovement
            };
        }
        
        public static MapDTO ToDTO(MapData mapData)
        {
            if (mapData == null)
                return null;

            int width = mapData.Width;
            int height = mapData.Height;

            int[] terrain = new int[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    GridCoordinate coordinate = new GridCoordinate(x, y);

                    TerrainTag terrainTag = mapData.GetTerrainAt(coordinate);

                    int index = y * width + x;

                    terrain[index] = (int)terrainTag;
                }
            }

            return new MapDTO
            {
                Width = width,
                Height = height,
                Terrain = terrain
            };
        }
        
        public static RoomDTO ToDTO(Room room)
        {
            if (room == null)
                return null;

            TokenDTO[] tokens = new TokenDTO[room.Tokens.Count];

            for (int i = 0; i < room.Tokens.Count; i++)
            {
                tokens[i] = ToDTO(room.Tokens[i]);
            }

            return new RoomDTO
            {
                Id = room.Id,
                Name = room.Name,
                Map = ToDTO(room.MapData),
                Tokens = tokens
            };
        }
        
        public static PlayerDTO ToDTO(SessionPlayer player)
        {
            if (player == null)
                return null;

            return new PlayerDTO
            {
                Id = player.Id,
                Name = player.Name,
                IsConnected = player.IsConnected,
                ControlledTokenIds = player.ControlledTokenIds.ToArray()
            };
        }
        
        public static SessionSnapshotDTO ToDTO(GameSession gameSession)
        {
            if (gameSession == null)
                return null;

            PlayerDTO[] players =
                new PlayerDTO[gameSession.Players.Count];

            for (int i = 0; i < gameSession.Players.Count; i++)
            {
                players[i] = ToDTO(gameSession.Players[i]);
            }

            return new SessionSnapshotDTO
            {
                ActiveRoomId = gameSession.ActiveRoom?.Id,
                ActiveRoom = ToDTO(gameSession.ActiveRoom),
                Players = players
            };
        }
    }
}