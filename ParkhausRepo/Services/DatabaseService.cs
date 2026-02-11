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
        private SQLiteAsyncConnection? _database;
        private bool _isInitialized = false;
         private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        // Singleton instance for backward compatibility
        private static DatabaseService? _instance;
        private static readonly object _instanceLock = new object();

        public static DatabaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseService();
                        }
                    }
                }
                return _instance;
            }
        }

        public DatabaseService()
        {
            // Don't initialize in constructor - do it lazily
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized)
                    return;

                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "parkhaus.db3");
                _database = new SQLiteAsyncConnection(dbPath);

                await _database.CreateTableAsync<Car>();
                await _database.CreateTableAsync<ParkingLot>();
                await _database.CreateTableAsync<ParkingSpace>();
                await CreateParkingLotAsync();

                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        // Create the Parking lot
        private async Task CreateParkingLotAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

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
                    IsOpen = true
                };
                await _database.InsertAsync(parkingLot);
            }
        }

        public async Task<ParkingLot> GetParkingLotAsync()
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<ParkingLot>().FirstOrDefaultAsync(); //Gets the first parking lot, there is only one so it works
        }

        public async Task UpdateParkingLotAsync(ParkingLot parkingLot)
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            await _database.UpdateAsync(parkingLot);
        }

        //Parking Spaces corresponding Database functions

        public async Task<List<ParkingSpace>> GetAllParkingSpacesAsync()
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<ParkingSpace>().ToListAsync();
        }

        public async Task<ParkingSpace> GetParkingSpaceByID(int id)
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var space = await _database.Table<ParkingSpace>().Where(s => s.ID == id).FirstOrDefaultAsync();
            return space;
        }

        public async Task<ParkingSpace> GetParkingSpaceAsync(int id)
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<ParkingSpace>().Where(s => s.ID == id).FirstOrDefaultAsync();
        }

        public async Task<ParkingSpace> GetAvailableParkingSpaceAsync()
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<ParkingSpace>().Where(s => !s.IsOccupied).FirstOrDefaultAsync();
        }

        public async Task<int> UpdateParkingSpaceAsync(ParkingSpace parkingSpace)
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.UpdateAsync(parkingSpace);
        }

        //All Car corresponding Database functions

        public async Task<List<Car>> GetAllCarsAsync()
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Car>().ToListAsync();
        }

        public async Task<Car> GetCarAsync(int id)
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Car>().Where(c => c.ID == id).FirstOrDefaultAsync();
        }

        public async Task<int> UpdateCarAsync(Car car)
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.UpdateAsync(car);
        }

        public async Task<int> InsertCarAsync(Car car)
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.InsertAsync(car);
        }

        public async Task<int> DeleteCarAsync(Car car)
        {
            await InitializeAsync();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.DeleteAsync(car);
        }
    }
}