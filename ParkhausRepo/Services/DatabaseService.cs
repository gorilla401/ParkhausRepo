using ParkhausRepo.Entities;
using ParkhausRepo.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParkhausRepo.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private static DatabaseService _instance;

        public static DatabaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseService();
                }
                return _instance;
            }
        }

        private DatabaseService()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "parkhaus.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            InitializeDatabaseAsync().Wait();
        }

        private async Task InitializeDatabaseAsync()
        {
            await _database.CreateTableAsync<Car>();
            await _database.CreateTableAsync<ParkingLot>();
            await _database.CreateTableAsync<ParkingSpace>();
        }


        // Create the Parking lot

        private async Task CreateParkingLotAsync()
        {
            var existingLot = await _database.Table<ParkingLot>().FirstOrDefaultAsync();
            if (existingLot == null)
            {
                var parkingLot = new ParkingLot
                {
                    Name = "Evan's Parking Lot",
                    Adress = "Andreas Heusler-Strasse 41",
                    Floors = 4,
                    TotalSpaces = 80,
                    OccupiedSpaces = 0,
                    AvailableSpaces = 80,
                    PricePerMinute = 0.05f,
                    IsOpen = true };
                await _database.InsertAsync(parkingLot);
            }
        }

        private async Task<ParkingLot> GetParkingLotAsync()
        {
            return await _database.Table<ParkingLot>().FirstOrDefaultAsync(); //Gets the first paring lot, there is only one so it works
        }

        private async Task UpdateParkingLotAsync(ParkingLot parkingLot)
        {
            await _database.UpdateAsync(parkingLot);
        }



        //All Car correspodning Database functions


    }
}
