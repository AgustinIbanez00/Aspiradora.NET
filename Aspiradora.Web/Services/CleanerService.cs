using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aspiradora.Web.Models;
using Aspiradora.Web.Controllers;
using Microsoft.AspNetCore.SignalR;

namespace Aspiradora.Web.Services;
/*
public class CleanerService
{
    public static CleanerService Instance = new CleanerService(true);
    


    public CleanerService(IHubContext<AspiradoraHub> hubContext)
    {

    }


    public CleanerService(bool load)
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



    private void Load()
    {
        if (List == null && Cleaners == null)
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
            foreach (Place place in row.Places)
            {
                if (place.State == PlaceState.DIRTY) count++;
            }
        }
        return count;
    }


    public bool CheckCleaner(Cleaner cleaner)
    {
        if (Cleaner.IsValid(cleaner))
        {
            foreach (PlaceRow placeRow in List)
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
*/