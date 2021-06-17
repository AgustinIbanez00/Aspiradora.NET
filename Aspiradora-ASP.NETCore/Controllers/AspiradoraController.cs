using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspiradora_ASP.NETCore.Models;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Aspiradora_ASP.NETCore.Controllers
{
    [HubName("AspiradoraHub")]
    public class AspiradoraController : Hub
    {
        public static IHubContext<AspiradoraController> _hubContext;

        private static readonly System.Timers.Timer _timer = new System.Timers.Timer();

        public AspiradoraController(IHubContext<AspiradoraController> hubContext)
        {
            _hubContext = hubContext;
        }

        static void StartTimer()
        {
            _timer.Interval = 1500;
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        static void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(CleanerModel.Instance != null)
            {
                foreach(Cleaner cleaner in CleanerModel.Instance.Cleaners)
                {
                    if (!cleaner.Controllable)
                    {
                        cleaner.Move((MovementDirection)new Random().Next(Enum.GetNames(typeof(MovementDirection)).Length), CleanerModel.Instance.Cleaners);
                    }
                    bool checker = CleanerModel.Instance.CheckCleaner(cleaner);
                    if (checker)
                    {
                        CleanerModel.Instance.List[cleaner.RowIndex.Value].Places[cleaner.ColumnIndex.Value].State = PlaceState.CLEAN;
                    }
                }
            }
        }


        public async Task Start(string NickName)
        {
            if(!string.IsNullOrEmpty(NickName) && !string.IsNullOrEmpty(Context.ConnectionId))
            {
                if (NickName.Length > 3)
                {
                    if (!CleanerModel.Instance.Cleaners.Exists(c => c.NickName.Equals(NickName, StringComparison.OrdinalIgnoreCase)))
                    {
                        CleanerModel.Instance.GenerateCleaner(Context.ConnectionId, NickName);

                        string list = JsonConvert.SerializeObject(CleanerModel.Instance.List);
                        string cleaners = JsonConvert.SerializeObject(CleanerModel.Instance.Cleaners);
                        await Clients.Caller.SendAsync("ReceiveStart", list, cleaners);
                        await Clients.All.SendAsync("ReceiveMessage", new { nickname = NickName, date = DateTime.Now.ToShortTimeString(), text = "se unió a la partida.", type = 1 });
                    }
                    else await Clients.Caller.SendAsync("ReceiveError", "Ya existe alguien con ese nombre.");
                }
                else await Clients.Caller.SendAsync("ReceiveError", "Colócate un nombre más largo.");
            }
            else await Clients.Caller.SendAsync("ReceiveError", "Flaco, ponete un nombre dije.");

            //if(!_timer.Enabled) StartTimer();
        }

        public async Task Move(int movement)
        {
            Debug.WriteLine("ConnectionID: (Start) " + Context.ConnectionId);
            if (!string.IsNullOrEmpty(Context.ConnectionId))
            {
                if (CleanerModel.Instance.Cleaners.Exists(c => c.Id == Context.ConnectionId))
                {
                    if(Enum.IsDefined(typeof(MovementDirection), movement))
                    {
                        Cleaner cleaner = CleanerModel.Instance.Cleaners.Find(c => c.Id == Context.ConnectionId);
                        cleaner.Move((MovementDirection)movement, CleanerModel.Instance.Cleaners);
                    }
                    else await Clients.Caller.SendAsync("ReceiveError", "¡Error grave!");
                }
                else await Clients.Caller.SendAsync("ReceiveError", "Tu aspiradora no existe.");
            }
            else await Clients.Caller.SendAsync("ReceiveError", "Ocurrió un error.");
        }

        public async Task Clear()
        {
            if (!string.IsNullOrEmpty(Context.ConnectionId))
            {
                if (CleanerModel.Instance.Cleaners.Exists(c => c.Id == Context.ConnectionId))
                {
                    Cleaner cleaner = CleanerModel.Instance.Cleaners.Find(c => c.Id == Context.ConnectionId);

                    cleaner.Clear();
                }
                else await Clients.Caller.SendAsync("ReceiveError", "Tu aspiradora no existe.");
            }
            else await Clients.Caller.SendAsync("ReceiveError", "Ocurrió un error.");
        }


        public async Task SendList()
        {
            string list = JsonConvert.SerializeObject(CleanerModel.Instance.List);
            Debug.WriteLine("Lista enviada.");
            await Clients.All.SendAsync("ReceiveList", list);
        }

        public async Task SendMessage(string message)
        {
            if(!string.IsNullOrEmpty(message))
            {
                if (CleanerModel.Instance.Cleaners.Exists(c => c.Id == Context.ConnectionId))
                {
                    Cleaner cleaner = CleanerModel.Instance.Cleaners.Find(c => c.Id == Context.ConnectionId);
                    await Clients.All.SendAsync("ReceiveMessage", new { nickname = cleaner.NickName, date = DateTime.Now.ToShortTimeString(), text = message, type = 0 });
                }
                else await Clients.Caller.SendAsync("ReceiveError", "Tu aspiradora no existe.");
            }
            else await Clients.Caller.SendAsync("ReceiveError", "Ocurrió un error.");
        }

        public async Task PlayAgain()
        {
            await Clients.Caller.SendAsync("ReceivePlay");
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Cleaner cleaner = CleanerModel.Instance.Cleaners.FirstOrDefault(c => c.Id == Context.ConnectionId);
            if(cleaner != null)
            {
                await Clients.All.SendAsync("ReceiveMessage", new { nickname = cleaner.NickName, date = DateTime.Now.ToShortTimeString(), text = "se desconectó de la partida.", type = 2 });
                await Clients.All.SendAsync("ReceiveDisconnect", cleaner.Id);
                CleanerModel.Instance.Cleaners.Remove(cleaner);

            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}