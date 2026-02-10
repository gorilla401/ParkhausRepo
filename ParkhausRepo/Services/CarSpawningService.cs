using ObjCRuntime;
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
            int spawnTime = _random.Next(minTime, maxTime) * 1000; //functional programming :)
            _spawnTimer = new Timer(async _ => await TrySpawningCarAsync(),
                                    null,
                                    spawnTime,
                                    Timeout.Infinite);
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
                    NumberPlate = _random.Next(67, 10000),
                    Color = CarColors[_random.Next(CarColors.Length)],
                    Brand  = CarBrands[_random.Next(CarBrands.Length)],
                    CurrentParkingSpace = availableSpace.ID,
                    CurrentParkingFloor = availableSpace.SpaceFloor,
                    ArrivalDate = DateTime.Now
                };

                await _databaseService.InsertCarAsync(car);

                availableSpace.IsOccupied = true;
                availableSpace.CurrentCarID =car.ID;
                await _databaseService.UpdateParkingSpaceAsync(availableSpace);

                    parkingLot.AvailableSpaces--;
                parkingLot.OccupiedSpaces++;
                await _databaseService.UpdateParkingLotAsync(parkingLot);

                CarSpawned?.Invoke(this, car) ;
            }
            _spawnTimer?.Change(TimeSpan.FromSeconds(_random.Next(7, 18)), Timeout.InfiniteTimeSpan);
        }

        public async Task<float> RemoveCarAsync(Car car)
        {
            var parkingSpace = await _databaseService.GetParkingSpaceByID(car.CurrentParkingSpace.Value);

            var arrivalTime = car.ArrivalDate;
            var departureTime = DateTime.Now;

            float earnings = (float)(arrivalTime - departureTime) * 0.05f;
            await _databaseService.DeleteCarAsync(car);

            parkingSpace.IsOccupied = false;
            parkingSpace.CurrentCarID = null;
            await _databaseService.UpdateParkingSpaceAsync(parkingSpace);

            var parkingLot = await _databaseService.GetParkingLotAsync();
            parkingLot.AvailableSpaces++;
            parkingLot.OccupiedSpaces--;

            await _databaseService.UpdateParkingLotAsync(parkingLot);

            return earnings;
        }
    }
}
