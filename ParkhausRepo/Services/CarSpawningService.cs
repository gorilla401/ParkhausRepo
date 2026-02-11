using ParkhausRepo.Models;

namespace ParkhausRepo.Services
{
    public class CarSpawningService
    {
        private readonly DatabaseService _databaseService;
        private Timer? _spawnTimer;
        private Random _random;
        private static readonly string[] CarColors = { "Red", "Blue", "Green", "Black" };
        private static readonly string[] CarBrands = { "VW", "BMW", "Honda", "Mercedes" };

        public event EventHandler<Car>? CarSpawned;
        public event EventHandler<Car>? CarLeft;

        // Constructor with dependency injection
        public CarSpawningService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _random = new Random();
        }

        public void StartSpawning(int minTime = 7, int maxTime = 18)
        {
            int spawnTime = _random.Next(minTime, maxTime) * 1000; //functional programming :)
            _spawnTimer = new Timer(async _ => await SpawnCarAsync(),
                                    null,
                                    spawnTime,
                                    Timeout.Infinite);
        }

        public void StopSpawning()
        {
            _spawnTimer?.Dispose();
        }

        private async Task SpawnCarAsync()
        {
            try
            {
                var parkingLot = await _databaseService.GetParkingLotAsync();

                if (parkingLot != null && parkingLot.AvailableSpaces > 0)
                {
                    var availableSpace = await _databaseService.GetAvailableParkingSpaceAsync();

                    if (availableSpace != null)
                    {
                        var car = new Car
                        {
                            NumberPlate = _random.Next(67, 10000),
                            Color = CarColors[_random.Next(CarColors.Length)],
                            Brand = CarBrands[_random.Next(CarBrands.Length)],
                            CurrentParkingSpace = availableSpace.ID,
                            CurrentParkingFloor = availableSpace.SpaceFloor,
                            ArrivalDate = DateTime.Now
                        };

                        await _databaseService.InsertCarAsync(car);

                        availableSpace.IsOccupied = true;
                        availableSpace.CurrentCarID =  car.ID;
                        await _databaseService.UpdateParkingSpaceAsync(availableSpace);

                        parkingLot.AvailableSpaces--;
                        parkingLot.OccupiedSpaces++;
                        await _databaseService.UpdateParkingLotAsync(parkingLot);

                        CarSpawned?.Invoke(this, car);
                    }
                }

                // Schedule next spawn
                _spawnTimer?.Change(TimeSpan.FromSeconds(_random.Next(7, 18)), Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error spawning car: {ex.Message}");
                // Retry spawning after error
                _spawnTimer?.Change(TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            }
        }

        public async Task<float> RemoveCarAsync(Car car)
        {
            if (!car.CurrentParkingSpace.HasValue)
            {
                throw new InvalidOperationException("Car does not have a parking space assigned.");
            }

            var parkingSpace = await _databaseService.GetParkingSpaceByID(car.CurrentParkingSpace.Value);

            if (parkingSpace == null)
            {
                throw new InvalidOperationException("Parking space not found.");
            }

            var arrivalTime = car.ArrivalDate;
            var departureTime = DateTime.Now;
            var parkingDuration = (departureTime - arrivalTime).TotalMinutes;
            var parkingLot = await _databaseService.GetParkingLotAsync();

            if (parkingLot == null)
            {
                throw new InvalidOperationException("Parking lot not found.");
            }

            float earnings = (float)parkingDuration * parkingLot.PricePerMinute;
            await _databaseService.DeleteCarAsync(car);

            parkingSpace.IsOccupied = false;
            parkingSpace.CurrentCarID = null;
            await _databaseService.UpdateParkingSpaceAsync(parkingSpace);

            parkingLot.AvailableSpaces++;
            parkingLot.OccupiedSpaces--;
            await _databaseService.UpdateParkingLotAsync(parkingLot);

            CarLeft?.Invoke(this, car);

            return earnings;
        }
    }
}