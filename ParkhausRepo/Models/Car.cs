using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParkhausRepo.Models
{
    public class Car
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int NumberPlate { get; set; }

        public DateTime ArrivalDate { get; set; }

        public DateTime? Departuredate { get; set; }

        public string Color { get; set; }

        public string Brand { get; set; }

        public int? CurrentParkingSpace { get; set; }

        public int? CurrentParkingFloor { get; set; }

        public int ParkingDuration { get; set; }
    }
}
