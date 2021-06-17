using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspiradora_ASP.NETCore.Controllers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Aspiradora_ASP.NETCore.Models
{
    public enum PlaceState
    {
        CLEAN,
        DIRTY
    }

    public class Place
    {
        public int Column { get; set; }
        public int Row { get; set; }
        private PlaceState state = PlaceState.CLEAN;
        public PlaceState State
        {
            get => state;
            set
            {
                state = value;
                AspiradoraController._hubContext.Clients.All.SendAsync("ReceiveCellChanged", Row, Column, state);
            }
        }

    }
}
