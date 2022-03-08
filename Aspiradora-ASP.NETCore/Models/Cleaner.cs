using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aspiradora.Web.Controllers;
using Microsoft.AspNetCore.SignalR;

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

    public class Cleaner
    {

        public string Id { get; set; }
        public string NickName { get; set; }
        public int MaxRows { get; set; }
        public int MaxColumns { get; set; }
        public bool Spectating { get; set; }
        internal int score { get; set; }
        public int Score {
            get => score;
            set
            {
                score = value;
                AspiradoraHub._hubContext.Clients.All.SendAsync("ReceiveScore", Id, score);
            }
        }
        internal int? rowIndex;
        public int? RowIndex
        {
            get => rowIndex;
            set
            {
                rowIndex = value;
                if (rowIndex.HasValue)
                {
                    AspiradoraHub._hubContext.Clients.All.SendAsync("CleanerRowChanged", Id, rowIndex);
                    Debug.WriteLine("Aspiradora " + NickName + " (" + Id.ToString() + ") se ha movido (RowIndex: " + rowIndex + ");");
                }
            }
        }
        internal int? columnIndex;
        public int? ColumnIndex
        {
            get => columnIndex;
            set
            {
                columnIndex = value;
                if (columnIndex.HasValue)
                {
                    AspiradoraHub._hubContext.Clients.All.SendAsync("CleanerColumnChanged", Id, columnIndex);
                    Debug.WriteLine("Aspiradora " + NickName + " (" + Id.ToString() + ") se ha movido (ColumnIndex: " + columnIndex + ");");
                }
            }
        }
        public bool Controllable { get; set; }
        internal Rotation rotation { get; set; }
        public Rotation Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                if(value != rotation)
                {
                    rotation = value;
                    AspiradoraHub._hubContext.Clients.All.SendAsync("ReceiveRotation", Id, rotation);
                    Debug.WriteLine("Aspiradora " + NickName + " (" + Id.ToString() + ") ha cambiado su rotación. (Rotation: " + rotation + ");");
                }
            }
        }

        public Cleaner(string connectionId)
        {
            Id = connectionId;
            NickName = "Cleaner";
            Controllable = false;
        }

        public void Move(MovementDirection direction, List<Cleaner> cleaners)
        {
            switch (direction)
            {
                case MovementDirection.UP:
                    Rotation = Rotation.NORTH;
                    if (RowIndex.HasValue && RowIndex.Value - 1 >= 0 && !cleaners.Exists(c => c.ColumnIndex == ColumnIndex && c.RowIndex == RowIndex - 1)) RowIndex--;
                    else Debug.WriteLine("Cleaner no se pudo mover hacia arriba. " + RowIndex + ", " + ColumnIndex);
                    break;
                case MovementDirection.DOWN:
                    Rotation = Rotation.SOUTH;                         
                    if (RowIndex.HasValue && RowIndex.Value + 1 < MaxRows && !cleaners.Exists(c => c.ColumnIndex == ColumnIndex && c.RowIndex == RowIndex + 1)) RowIndex++;
                    else Debug.WriteLine("Cleaner no se pudo mover hacia abajo. " + RowIndex + ", " + ColumnIndex);
                    break;
                case MovementDirection.LEFT:
                    Rotation = Rotation.WEST;
                    if (ColumnIndex.HasValue && ColumnIndex.Value - 1 >= 0 && !cleaners.Exists(c => c.ColumnIndex == ColumnIndex - 1 && c.RowIndex == RowIndex)) ColumnIndex--;
                    else Debug.WriteLine("Cleaner no se pudo mover hacia izquierda. " + RowIndex + ", " + ColumnIndex);

                    break;
                case MovementDirection.RIGHT:
                    Rotation = Rotation.EAST;
                    if (ColumnIndex.HasValue && ColumnIndex.Value + 1 < MaxColumns && !cleaners.Exists(c => c.ColumnIndex == ColumnIndex + 1 && c.RowIndex == RowIndex - 1)) ColumnIndex++;
                    else Debug.WriteLine("Cleaner no se pudo mover hacia derecha. " + RowIndex + ", " + ColumnIndex);
                    break;
                default:
                    break;
            }
        }

        public void Clear()
        {
            if(IsValid(this) && CleanerModel.Instance.List != null)
            {
                if (CleanerModel.Instance.CheckCleaner(this))
                {
                    CleanerModel.Instance.List[RowIndex.Value].Places[ColumnIndex.Value].State = PlaceState.CLEAN;
                    Score++;
                    if(CleanerModel.Instance.CountDirt() == 0)
                    {
                        Cleaner winner = CleanerModel.Instance.Cleaners.OrderByDescending(c => c.Score).FirstOrDefault();
                        AspiradoraHub._hubContext.Clients.All.SendAsync("ReceiveWinner", winner.Id);
                    }
                }
            }
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
       

        public static bool IsValid(Cleaner cleaner)
        {
            return cleaner != null && cleaner.ColumnIndex.HasValue && cleaner.RowIndex.HasValue;
        }
    }

}
