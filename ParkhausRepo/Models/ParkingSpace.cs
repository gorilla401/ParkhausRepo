using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParkhausRepo.Models
{
    public class ParkingSpace
    {
        [PrimaryKey, AutoIncrement]

        public int ID { get; set; }

        public int SpaceNumber { get; set; }

        public int SpaceFloor { get; set; }

        public bool IsOccupied { get; set; }

        public int? CurrentCarID { get; set; }
    }
}
