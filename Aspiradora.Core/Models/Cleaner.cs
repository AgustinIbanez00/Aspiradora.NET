using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Aspiradora.Web.Models
{
    public enum MovementDirection
    {
        UP,
        RIGHT,
        DOWN,
        LEFT

    }

    public enum RotateDirection
    {
        LEFT,
        RIGHT
    }


    public enum Rotation
    {
        NORTH,
        EAST,
        SOUTH,
        WEST
    }

    public class Cleaner : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string NickName { get; set; }
        public int MaxRows { get; set; }
        public int MaxColumns { get; set; }
        public bool Spectating { get; set; }
        public int Score { get; set; }
        public int? RowIndex { get; set; }
        public int? ColumnIndex { get; set; }
        public bool Controllable { get; set; }
        public Rotation Rotation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public Cleaner()
        {

        }

        public Cleaner(string connectionId)
        {
            Id = connectionId;
            NickName = "Cleaner";
            Controllable = false;
        }

        public Rotation Rotate(RotateDirection direction)
        {
            switch (direction)
            {
                case RotateDirection.LEFT:
                    switch (Rotation)
                    {
                        case Rotation.NORTH:
                            Rotation = Rotation.WEST;
                            break;
                        case Rotation.EAST:
                            Rotation = Rotation.NORTH;
                            break;
                        case Rotation.SOUTH:
                            Rotation = Rotation.EAST;
                            break;
                        case Rotation.WEST:
                            Rotation = Rotation.SOUTH;
                            break;
                        default:
                            break;
                    }
                    break;
                case RotateDirection.RIGHT:
                    switch (Rotation)
                    {
                        case Rotation.NORTH:
                            Rotation = Rotation.EAST;
                            break;
                        case Rotation.EAST:
                            Rotation = Rotation.SOUTH;
                            break;
                        case Rotation.SOUTH:
                            Rotation = Rotation.WEST;
                            break;
                        case Rotation.WEST:
                            Rotation = Rotation.NORTH;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return Rotation;
        }

        public bool IsValid()
        {
            return ColumnIndex.HasValue && RowIndex.HasValue;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
