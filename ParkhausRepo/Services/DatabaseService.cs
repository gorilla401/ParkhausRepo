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
            await CreateParkingLotAsync();
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
                    Adress = "Andreas Heusler-Strasse, 41",
                    Floors = 4,
                    TotalSpaces = 80,
                    OccupiedSpaces = 0,
                    AvailableSpaces = 80,
                    PricePerMinute = 0.05f,
                    IsOpen = true };
                await _database.InsertAsync(parkingLot);
            }
        }

        public async Task<ParkingLot> GetParkingLotAsync()
        {
            return await _database.Table<ParkingLot>().FirstOrDefaultAsync(); //Gets the first paring lot, there is only one so it works
        }

        public async Task UpdateParkingLotAsync(ParkingLot parkingLot)
        {
            await _database.UpdateAsync(parkingLot);
        }

        //Parking Spaces corresponding Database functions

        public async Task <List<ParkingSpace>> GetAllParkingSpacesAsync()
        {
            return await _database.Table<ParkingSpace>().ToListAsync();
        }

        public async Task<ParkingSpace> GetParkingSpaceAsync(int id)
        {
            return await _database.Table<ParkingSpace>().Where(s => s.ID == id).FirstOrDefaultAsync();
        }

        public async Task<ParkingSpace> GetAvailableParkingSpaceAsync()
        {
            return await _database.Table<ParkingSpace>().Where(s => !s.IsOccupied).FirstOrDefaultAsync();
        }

        public async Task<int> UpdateParkingSpaceAsync(ParkingSpace parkingSpace)
        {
            return await _database.UpdateAsync(parkingSpace);
        }

        //All Car correspodning Database functions

        public async Task<List<Car>> GetAllCarsAsync()
        {
            return await _database.Table<Car>().ToListAsync();
        }

        public async Task<Car> GetCarAsync(int id)
        {
            return await _database.Table<Car>().Where(c => c.ID == id).FirstOrDefaultAsync();
        }

        public async Task<int> UpdateCarAsync(Car car)
        {
            return await _database.UpdateAsync(car);
        }
    }
}
