using Aspiradora.Web.Models;

namespace Aspiradora.Core.Models;
public class GameManager
{
    private List<GameRoom> _games { get; set; }

    public GameManager()
    {
        _games = new List<GameRoom>();
    }

    public string CreateGame(string connectionId)
    {
        var newRoom = new GameRoom()
        {
            GameId = Guid.NewGuid(),
            OwnerId = connectionId,
            Started = false
        };
        newRoom.GeneratePlaces();
        _games.Add(newRoom);

        return newRoom.GameId.ToString();
    }

    public GameRoom FindGame(Guid guid) => _games.Find(x => x.GameId == guid);

    public GameRoom FindGame(string guid) => _games.Find(x => x.GameId.ToString() == guid);

    public bool DeleteGame(string guid)
    {
        var gameRoom = FindGame(guid);

        if (gameRoom == null)
            return false;

        return gameRoom.Players.Count == 0 ? _games.Remove(gameRoom) : false;
    }

}
