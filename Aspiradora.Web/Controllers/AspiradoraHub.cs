using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Aspiradora.Web.Models;
using Microsoft.AspNetCore.SignalR;
using Aspiradora.Core;
using Aspiradora.Core.Models;
using System.Linq;

namespace Aspiradora.Web.Controllers;
public class AspiradoraHub : Hub<IAspiradoraClient>
{
    private static readonly System.Timers.Timer _timer = new System.Timers.Timer();
    private readonly GameManager _gameManager;
    private readonly string SESSION_KEY = "sessionId";
    private readonly string NICKNAME_KEY = "nickName";


    public AspiradoraHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    static void StartTimer()
    {
        _timer.Interval = 1500;
        //_timer.Elapsed += TimerElapsed;
        _timer.Start();
    }
    /*
    static void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (CleanerService.Instance != null)
        {
            foreach (Cleaner cleaner in CleanerService.Instance.Cleaners)
            {
                if (!cleaner.Controllable)
                {
                    cleaner.Move((MovementDirection)new Random().Next(Enum.GetNames(typeof(MovementDirection)).Length), CleanerService.Instance.Cleaners);
                }
                bool checker = CleanerService.Instance.CheckCleaner(cleaner);
                if (checker)
                {
                    CleanerService.Instance.List[cleaner.RowIndex.Value].Places[cleaner.ColumnIndex.Value].State = PlaceState.CLEAN;
                }
            }
        }
    }*/

    public async Task JoinGame(string sessionId, string nickName)
    {
        if (InSession)
        {
            await Clients.Caller.ReceiveErrorAsync("Ya estás en una sala.");
            return;
        }
        else
        {
            if (string.IsNullOrEmpty(nickName))
            {
                await Clients.Caller.ReceiveErrorAsync("Necesitas un nick para jugar!");
                return;
            }

            if (nickName.Length < 3)
            {
                await Clients.Caller.ReceiveErrorAsync("No puedes tener un nombre muy corto.");
                return;
            }

            await Start(sessionId, nickName);
        }

    }

    public bool InSession => Context.Items.ContainsKey(SESSION_KEY);

    public string Session => InSession ? (string)Context.Items[SESSION_KEY] : null;

    public bool HasNickName => Context.Items.ContainsKey(NICKNAME_KEY);

    public string NickName => HasNickName ? (string)Context.Items[NICKNAME_KEY] : null;


    public async Task CreateSession(string nickName)
    {
        if(InSession)
        {
            await Clients.Caller.ReceiveErrorAsync("Ya estás en una sala.");
            return;
        }
        else
        {
            if (string.IsNullOrEmpty(nickName))
            {
                await Clients.Caller.ReceiveErrorAsync("Necesitas un nick para jugar!");
                return;
            }

            if (nickName.Length < 3)
            {
                await Clients.Caller.ReceiveErrorAsync("No puedes tener un nombre muy corto.");
                return;
            }

            var newGameId = _gameManager.CreateGame(Context.ConnectionId);
            await Start(newGameId, nickName);
        }

        return;
    }

    private async Task Start(string sessionId, string nickName)
    {
        var gameRoom = _gameManager.FindGame(sessionId);

        if(gameRoom == null)
        {
            await Clients.Caller.ReceiveErrorAsync("La sala no existe o no está disponible.");
            return;
        }

        if(gameRoom.ExistsPlayer(nickName))
        {
            await Clients.Caller.ReceiveErrorAsync("Hay un jugador con el mismo nombre. Elige otro.");
            _gameManager.DeleteGame(sessionId);
            return;
        }

        var newCleaner = gameRoom.GenerateCleaner(Context.ConnectionId, nickName);

        if(newCleaner == null)
        {
            await Clients.Caller.ReceiveErrorAsync("No te pudiste unir a la partida.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        Context.Items.Add(SESSION_KEY, sessionId);
        Context.Items.Add(NICKNAME_KEY, nickName);

        await Clients.Group(Session).ReceiveMessageAsync(nickName, DateTime.Now.ToString(), "Se unió a la partida.", 1);
        await Clients.Group(Session).ReceiveCleanerAsync(JsonSerializer.Serialize(newCleaner));
        await Clients.Group(Session).ReceiveStartAsync(Session, JsonSerializer.Serialize(gameRoom.Places), JsonSerializer.Serialize(gameRoom.Players));
        //if(!_timer.Enabled) StartTimer();
    }

    public async Task Move(MovementDirection movement)
    {
        if(!InSession)
        {
            await Clients.Caller.ReceiveErrorAsync("No te encuentras en una sala.");
            return;
        }

        var gameRoom = _gameManager.FindGame(Session);

        if (gameRoom == null)
        {
            await Clients.Caller.ReceiveErrorAsync("La sala no existe o no está disponible.");
            return;
        }

        Debug.WriteLine("ConnectionID: (Start) " + Context.ConnectionId);
        if (!string.IsNullOrEmpty(Context.ConnectionId))
        {
            if (gameRoom.ExistsPlayer(NickName))
            {
                if (Enum.IsDefined(typeof(MovementDirection), movement))
                {
                    var cleaner = gameRoom.MoveCleaner(Context.ConnectionId, movement);

                    await Clients.Group(Session).ReceiveRotationAsync(cleaner.Id, cleaner.Rotation);
                    if(cleaner.ColumnIndex.HasValue)
                        await Clients.Group(Session).CleanerColumnChangedAsync(cleaner.Id, cleaner.ColumnIndex.Value);
                    if(cleaner.RowIndex.HasValue)
                        await Clients.Group(Session).CleanerRowChangedAsync(cleaner.Id, cleaner.RowIndex.Value);
                }
                else await Clients.Caller.ReceiveErrorAsync("¡Error grave!");
            }
            else await Clients.Caller.ReceiveErrorAsync("Tu aspiradora no existe.");
        }
        else await Clients.Caller.ReceiveErrorAsync("Ocurrió un error.");
    }

    public async Task Clear()
    {
        if (!InSession)
        {
            await Clients.Caller.ReceiveErrorAsync("No te encuentras en una sala.");
            return;
        }

        var gameRoom = _gameManager.FindGame(Session);

        if (gameRoom == null)
        {
            await Clients.Caller.ReceiveErrorAsync("La sala no existe o no está disponible.");
            return;
        }

        var cleaner = gameRoom.FindPlayer(Context.ConnectionId);

        if (cleaner == null)
        {
            await Clients.Caller.ReceiveErrorAsync("No estás jugando en la partida.");
            return;
        }

        if(cleaner.RowIndex.HasValue && cleaner.ColumnIndex.HasValue && gameRoom.CleanerClean(cleaner.Id))
        {
            await Clients.Group(Session).ReceiveCellChangedAsync(cleaner.RowIndex.Value, cleaner.ColumnIndex.Value, gameRoom.Places[cleaner.RowIndex.Value].Places[cleaner.ColumnIndex.Value].State);
            await Clients.Group(Session).ReceiveScoreAsync(cleaner.Id, cleaner.Score);
            if (gameRoom.Places.Count(c => c.Places.Any(p => p.State == PlaceState.DIRTY)) == 0)
            {
                Cleaner winner = gameRoom.Players.OrderByDescending(c => c.Score).FirstOrDefault();
                await Clients.Group(Session).ReceiveWinnerAsync(winner.Id);
                //AspiradoraHub._hubContext.Clients.All.SendAsync("ReceiveWinner", winner.Id);
            }
        }
    }


    public async Task SendList()
    {
        if (!InSession)
        {
            await Clients.Caller.ReceiveErrorAsync("No te encuentras en una sala.");
            return;
        }

        var gameRoom = _gameManager.FindGame(Session);

        if (gameRoom == null)
        {
            await Clients.Caller.ReceiveErrorAsync("La sala no existe o no está disponible.");
            return;
        }

        await Clients.Group(Session).ReceiveListAsync(JsonSerializer.Serialize(gameRoom.Places));
    }

    public async Task SendMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            await Clients.Caller.ReceiveErrorAsync("El mensaje está vacío.");
            return;
        }

        if (!InSession)
        {
            await Clients.Caller.ReceiveErrorAsync("No te encuentras en una sala.");
            return;
        }

        var gameRoom = _gameManager.FindGame(Session);

        if (gameRoom == null)
        {
            await Clients.Caller.ReceiveErrorAsync("La sala no existe o no está disponible.");
            return;
        }

        await Clients.Group(Session).ReceiveMessageAsync(NickName, DateTime.UtcNow.ToString(), message, 0);
    }

    public async Task PlayAgain()
    {/*
        await Clients.Caller.SendAsync("ReceivePlay");
        */
    }

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (!InSession)
            return;

        var gameRoom = _gameManager.FindGame(Session);

        if (gameRoom == null)
            return;

        var cleaner = gameRoom.FindPlayer(Context.ConnectionId);

        if (cleaner == null)
            return;

        await Clients.Group(Session).ReceiveMessageAsync(NickName, DateTime.Now.ToString(), "Se desconectó de la partida", 2);
        await Clients.Group(Session).ReceiveDisconnectAsync(cleaner.Id);

        gameRoom.RemovePlayer(cleaner.Id);

        Context.Items.Remove(SESSION_KEY);
        Context.Items.Remove(NICKNAME_KEY);



        await base.OnDisconnectedAsync(exception);
    }
}
