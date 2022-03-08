using Aspiradora.Web.Controllers;
using Microsoft.AspNetCore.SignalR;

namespace Aspiradora.Web.Models;
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
            AspiradoraHub._hubContext.Clients.All.SendAsync("ReceiveCellChanged", Row, Column, state);
        }
    }

}
