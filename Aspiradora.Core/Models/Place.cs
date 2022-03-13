using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aspiradora.Web.Models;
public enum PlaceState
{
    CLEAN,
    DIRTY
}

public class Place : INotifyPropertyChanged
{
    public int Column { get; set; }
    public int Row { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    public PlaceState State { get; set; }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
