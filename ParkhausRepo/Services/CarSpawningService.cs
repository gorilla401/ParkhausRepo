using ParkhausRepo.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParkhausRepo.Services
{
    public class CarSpawningService
    {
        private readonly DatabaseService _databaseService;
        private Timer _spawnTimer;
        private Random _random;
        private static readonly string[] CarColors = { "Red", "Blue", "Green", "Black"};
        private static readonly string[] CarBrands = { "VW", "BMW", "Honda", "Mercedes" };

        public event EventHandler<Car> CarSpawned;
        public event EventHandler<Car> CarLeft;

        public CarSpawningService()
        {
            _databaseService = DatabaseService.Instance;
            _random = new Random();
        }

        public void StartSpawning(int minTime = 7, int maxTime = 18)
        {
            int spawnTime = _random.Next(minTime, maxTime) * 1000;
        }

        public void StopSpawning()
        {
            _spawnTimer?.Dispose();
        }

        private async Task TrySpawningCarAsync()
        {
            var parkingLot = await _databaseService.GetParkingLotAsync();

            if (parkingLot.AvailableSpaces > 0)
            {
                var availableSpace = await _databaseService.GetAvailableParkingSpaceAsync();

                var car = new Car
                {
                    NumberPlate = _random.Next(1000, 9999),
                    Color = CarColors[_random.Next(CarColors.Length)],
                    Brand = CarBrands[_random.Next(CarBrands.Length)],
                    CurrentParkingSpace = availableSpace.ID,
                    CurrentParkingFloor = availableSpace.SpaceFloor,
                    ArrivalDate = DateTime.Now
                };

                await _databaseService.InsertCarAsync(car);

                availableSpace
            }
        }

    }
}
