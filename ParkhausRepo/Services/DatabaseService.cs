using ParkhausRepo.Models;  // Changed from Entities
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

        // Create the Parking lot and parking spaces
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

                // Create initial parking spaces (80 spaces across 4 floors)
                await CreateInitialParkingSpacesAsync();
                
                // Create default cars
                await CreateDefaultCarsAsync();
            }
        }

        private async Task CreateDefaultCarsAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            // Check if cars already exist
            var existingCars = await _database.Table<Car>().CountAsync();
            if (existingCars > 0)
                return;

            // Create 5 default cars
            var defaultCars = new List<Car>
            {
                new Car
                {
                    NumberPlate = 1234,
                    Color = "Red",
                    Brand = "BMW",
                    ArrivalDate = DateTime.Now.AddHours(-2),
                    CurrentParkingFloor = 1
                },
                new Car
                {
                    NumberPlate = 5678,
                    Color = "Blue",
                    Brand = "Mercedes",
                    ArrivalDate = DateTime.Now.AddHours(-5),
                    CurrentParkingFloor = 1
                },
                new Car
                {
                    NumberPlate = 9012,
                    Color = "Black",
                    Brand = "VW",
                    ArrivalDate = DateTime.Now.AddHours(-1),
                    CurrentParkingFloor = 2
                },
                new Car
                {
                    NumberPlate = 3456,
                    Color = "Green",
                    Brand = "Honda",
                    ArrivalDate = DateTime.Now.AddHours(-3),
                    CurrentParkingFloor = 2
                },
                new Car
                {
                    NumberPlate = 7890,
                    Color = "Red",
                    Brand = "VW",
                    ArrivalDate = DateTime.Now.AddHours(-4),
                    CurrentParkingFloor = 3
                }
            };

            // Get the first 5 available parking spaces
            var availableSpaces = await _database.Table<ParkingSpace>()
                .Where(s => !s.IsOccupied)
                .Take(5)
                .ToListAsync();

            // Assign parking spaces to cars and insert them
            for (int i = 0; i < defaultCars.Count && i < availableSpaces.Count; i++)
            {
                var car = defaultCars[i];
                var space = availableSpaces[i];

                car.CurrentParkingSpace = space.ID;
                car.CurrentParkingFloor = space.SpaceFloor;

                await _database.InsertAsync(car);

                // Mark the space as occupied
                space.IsOccupied = true;
                space.CurrentCarID = car.ID;
                await _database.UpdateAsync(space);
            }

            // Update parking lot statistics
            var parkingLot = await _database.Table<ParkingLot>().FirstOrDefaultAsync();
            if (parkingLot != null)
            {
                parkingLot.OccupiedSpaces = defaultCars.Count;
                parkingLot.AvailableSpaces = parkingLot.TotalSpaces - defaultCars.Count;
                await _database.UpdateAsync(parkingLot);
            }
        }

        private async Task CreateInitialParkingSpacesAsync()
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var existingSpaces = await _database.Table<ParkingSpace>().CountAsync();
            if (existingSpaces > 0)
                return;

            
            int spacesPerFloor = 20;
            int totalFloors = 4;

            for (int floor = 1; floor <= totalFloors; floor++)
            {
                for (int spaceNum = 1; spaceNum <= spacesPerFloor; spaceNum++)
                {
                    var parkingSpace = new ParkingSpace
                    {
                        SpaceNumber = ((floor - 1) * spacesPerFloor) + spaceNum,
                        SpaceFloor = floor,
                        IsOccupied = false,
                    };
                    await _database.InsertAsync(parkingSpace);
                }
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

            var availableSpaces = await _database.Table<ParkingSpace>()
                .Where(s => !s.IsOccupied)
                .ToListAsync();

            if (availableSpaces.Count == 0)
                return null;

            var random = new Random();
            return availableSpaces[random.Next(availableSpaces.Count)];
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