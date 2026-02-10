using ParkhausRepo.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParkhausRepo.Services
{
    public class CarSpawningService
    {
        private readonly DatabaseService _databaseService;
        private Timer _timer;
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
    }
}
