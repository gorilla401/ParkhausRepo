using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ParkhausRepo.Models  // Changed from ParkhausRepo.Entities
{
    public class ParkingLot
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; } = 1;

        public string Name { get; set; } = string.Empty;
        public string Adress { get; set; } = string.Empty;

        public int Floors { get; set; }
        public int TotalSpaces { get; set; }

        public int OccupiedSpaces { get; set; }

        public int AvailableSpaces { get; set; }

        public float PricePerMinute { get; set; } = 0.05f;

        public bool IsOpen { get; set; } = true;

        public float TotalEarnings { get; set; } = 0f;
    }
}
