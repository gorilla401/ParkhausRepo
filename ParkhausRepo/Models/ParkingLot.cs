using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ParkhausRepo.Entities
{
    public class ParkingLot
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; } = 1; //1 because it's always the same lot

        public string Name { get; set; } = string.Empty;
        public  string Adress { get; set; } = string.Empty;

        public int Floors { get; set; }
        public int TotalSpaces { get; set; }

        public int OccupiedSpaces { get; set; }

        public int AvailableSpaces { get; set; }

        public float PricePerMinute { get; set; } = 0.05f; // Default price per minute f because it's always this.

        public bool IsOpen { get; set; } = true;
    }
}
