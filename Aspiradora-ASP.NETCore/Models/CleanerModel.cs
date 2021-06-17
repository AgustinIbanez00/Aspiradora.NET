using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Aspiradora_ASP.NETCore.Controllers;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Aspiradora_ASP.NETCore.Models
{
    public class CleanerModel
    {
        public static CleanerModel Instance = new CleanerModel(true);
        private const bool DEFAULT_CONTROLLABLE = true;

        public List<PlaceRow> List = null;

        public Random Random = new Random();

        public int COLUMNS { get { return 10; } }
        public int ROWS { get { return 10; } }
        public int POSSIBILTY { get { return Random.Next(50); } }

        public bool Started { get; set; }

        public List<Cleaner> Cleaners = null;
        
        public CleanerModel(bool load)
        {
            if (load) Load();
        }

        void AddPlaces()
        {
            List.Clear();
            Cleaners.Clear();
            for (int i = 0; i < ROWS; i++)
            {
                List<Place> places = new List<Place>();
                for (int j = 0; j < COLUMNS; j++) places.Add(new Place() { Row = i, Column = j, State = CalculatePlaceState(POSSIBILTY) });
                List.Add(new PlaceRow() { Places = places });
            }
        }

        PlaceState CalculatePlaceState(int value)
        {
            int randResult = Random.Next(0, 100);
            if (randResult >= 0 && randResult <= Convert.ToInt32(value)) return PlaceState.DIRTY;
            else return PlaceState.CLEAN;
        }

        public void GenerateCleaner(string connection_id, string nick, int recursive_index = 0)
        {
            if (Cleaners.Count <= 10 && recursive_index < 10)
            {
                int randRow = Random.Next(0, ROWS);
                int randCol = Random.Next(0, COLUMNS);

                if (!Cleaners.Exists(c => (nick != null && c.NickName == nick || nick == null) && c.RowIndex == randRow && c.ColumnIndex == randCol))
                {
                    Cleaner cleaner = new Cleaner(connection_id) { NickName = nick, ColumnIndex = randCol, RowIndex = randRow, Controllable = DEFAULT_CONTROLLABLE, MaxColumns = COLUMNS, MaxRows = ROWS, Rotation = Rotation.NORTH, Spectating = Started };
                    Cleaners.Add(cleaner);
                    AspiradoraController._hubContext.Clients.All.SendAsync("ReceiveCleaner", JsonConvert.SerializeObject(cleaner));
                    Debug.WriteLine("Aspiradora generada.");
                }
                else GenerateCleaner(nick, connection_id, recursive_index++);
            }
            else Debug.WriteLine("Se alcanzó el límite de aspiradoras.");
        }

        private void Load()
        {
            if(List == null && Cleaners == null)
            {
                List = new List<PlaceRow>();
                Cleaners = new List<Cleaner>();
                AddPlaces();
                //GenerateCleaner();
            }
        }

        public int CountDirt()
        {
            int count = 0;
            foreach (PlaceRow row in List)
            {
                foreach(Place place in row.Places)
                {
                    if (place.State == PlaceState.DIRTY) count++;
                }
            }
            return count;
        }


        public bool CheckCleaner(Cleaner cleaner)
        {
            if(Cleaner.IsValid(cleaner))
            {
                foreach(PlaceRow  placeRow in List)
                {
                    foreach (Place place in placeRow.Places)
                    {
                        if (place.Column == cleaner.ColumnIndex && place.Row == cleaner.RowIndex && place.State == PlaceState.DIRTY)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;

        }



    }
}
