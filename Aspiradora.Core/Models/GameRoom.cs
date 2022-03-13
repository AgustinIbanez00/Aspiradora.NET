using System.Diagnostics;
using Aspiradora.Web.Models;

namespace Aspiradora.Core.Models
{
    public class GameRoom
    {
        public GameRoom()
        {
            Places = new List<PlaceRow>();
            Players = new List<Cleaner>();
        }

        private const bool DEFAULT_CONTROLLABLE = true;

        public Guid GameId { get; set; }
        public string OwnerId { get; set; }

        public Random Seed = new Random();

        public int COLUMNS { get { return 10; } }
        public int ROWS { get { return 10; } }
        public int POSSIBILTY { get { return Seed.Next(50); } }

        public bool Started { get; set; }
        public List<Cleaner> Players { get; private set; }
        public List<PlaceRow> Places { get; private set; }

        public bool ExistsPlayer(string nickName) => Players.Exists(c => c.NickName.Equals(nickName, StringComparison.OrdinalIgnoreCase));

        public Cleaner FindPlayer(string connectionId) => Players.Find(p => p.Id == connectionId);

        public Cleaner GenerateCleaner(string connection_id, string nick, int recursive_index = 0)
        {
            if (Players.Count <= 10 && recursive_index < 10)
            {
                int randRow = Seed.Next(0, ROWS);
                int randCol = Seed.Next(0, COLUMNS);

                if (!Players.Exists(c => (nick != null && c.NickName == nick || nick == null) && c.RowIndex == randRow && c.ColumnIndex == randCol))
                {
                    Cleaner cleaner = new Cleaner(connection_id) { NickName = nick, ColumnIndex = randCol, RowIndex = randRow, Controllable = DEFAULT_CONTROLLABLE, MaxColumns = COLUMNS, MaxRows = ROWS, Rotation = Rotation.NORTH, Spectating = Started };
                    Players.Add(cleaner);
                    return cleaner;
                }
                else GenerateCleaner(nick, connection_id, recursive_index++);
            }
            return null;
        }

        public void RemovePlayer(string connectionId)
        {
            Cleaner cleaner = FindPlayer(connectionId);

            if (cleaner == null)
                return;

            Players.Remove(cleaner);
        }

        public Cleaner MoveCleaner(string connectionId, MovementDirection direction)
        {
            Cleaner cleaner = FindPlayer(connectionId);

            if (cleaner == null)
                return null;

            switch (direction)
            {
                case MovementDirection.UP:
                    cleaner.Rotation = Rotation.NORTH;
                    if (cleaner.RowIndex.HasValue && cleaner.RowIndex.Value - 1 >= 0 && !Players.Exists(c => c.ColumnIndex == cleaner.ColumnIndex && c.RowIndex == cleaner.RowIndex - 1))
                        cleaner.RowIndex--;
                    else Debug.WriteLine("Cleaner no se pudo mover hacia arriba. " + cleaner.RowIndex + ", " + cleaner.ColumnIndex);
                    break;
                case MovementDirection.DOWN:
                    cleaner.Rotation = Rotation.SOUTH;
                    if (cleaner.RowIndex.HasValue && cleaner.RowIndex.Value + 1 < cleaner.MaxRows && !Players.Exists(c => c.ColumnIndex == cleaner.ColumnIndex && c.RowIndex == cleaner.RowIndex + 1))
                        cleaner.RowIndex++;
                    else Debug.WriteLine("Cleaner no se pudo mover hacia abajo. " + cleaner.RowIndex + ", " + cleaner.ColumnIndex);
                    break;
                case MovementDirection.LEFT:
                    cleaner.Rotation = Rotation.WEST;
                    if (cleaner.ColumnIndex.HasValue && cleaner.ColumnIndex.Value - 1 >= 0 && !Players.Exists(c => c.ColumnIndex == cleaner.ColumnIndex - 1 && c.RowIndex == cleaner.RowIndex))
                        cleaner.ColumnIndex--;
                    else Debug.WriteLine("Cleaner no se pudo mover hacia izquierda. " + cleaner.RowIndex + ", " + cleaner.ColumnIndex);
                    break;
                case MovementDirection.RIGHT:
                    cleaner.Rotation = Rotation.EAST;
                    if (cleaner.ColumnIndex.HasValue && cleaner.ColumnIndex.Value + 1 < cleaner.MaxColumns && !Players.Exists(c => c.ColumnIndex == cleaner.ColumnIndex + 1 && c.RowIndex == cleaner.RowIndex))
                        cleaner.ColumnIndex++;
                    else Debug.WriteLine("Cleaner no se pudo mover hacia derecha. " + cleaner.RowIndex + ", " + cleaner.ColumnIndex);
                    break;
                default:
                    break;
            }
            return cleaner;
        }

        public bool CleanerClean(string connectionId)
        {
            Cleaner cleaner = FindPlayer(connectionId);

            if (cleaner == null)
                return false;

            if (cleaner.IsValid() && Places != null)
            {
                if (CheckCleaner(cleaner))
                {
                    Places[cleaner.RowIndex.Value].Places[cleaner.ColumnIndex.Value].State = PlaceState.CLEAN;
                    cleaner.Score++;

                    return true;
                }
            }
            return false;
        }
        /*
        private int CountDirt()
        {
            int count = 0;
            foreach (PlaceRow row in List)
            {
                foreach (Place place in row.Places)
                {
                    if (place.State == PlaceState.DIRTY) count++;
                }
            }
            return count;
        }*/

        private bool CheckCleaner(Cleaner cleaner)
        {
            return cleaner.IsValid() && Places.Any(p => p.Places.Any(place => place.Column == cleaner.ColumnIndex && place.Row == cleaner.RowIndex && place.State == PlaceState.DIRTY));
        }

        public void GeneratePlaces()
        {
            Places.Clear();
            Players.Clear();
            for (int i = 0; i < ROWS; i++)
            {
                List<Place> places = new List<Place>();
                for (int j = 0; j < COLUMNS; j++) places.Add(new Place() { Row = i, Column = j, State = CalculatePlaceState(POSSIBILTY) });
                Places.Add(new PlaceRow() { Places = places });
            }
        }

        private PlaceState CalculatePlaceState(int value)
        {
            int randResult = Seed.Next(0, 100);
            return randResult >= 0 && randResult <= Convert.ToInt32(value) ? PlaceState.DIRTY : PlaceState.CLEAN;
        }
    }
}
