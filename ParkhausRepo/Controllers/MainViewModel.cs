using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParkhausRepo.Services;
using ParkhausRepo.Models;
using System.Collections.ObjectModel;

namespace ParkhausRepo.Controllers
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly CarSpawningService _carSpawningService;

        [ObservableProperty]
        private ParkingLot? _parkingLot;

        [ObservableProperty]
        private ObservableCollection<Car> _cars = new();

        [ObservableProperty]
        private int _totalSpaces;

        [ObservableProperty]
        private int _availableSpaces;

        [ObservableProperty]
        private int _occupiedSpaces;

        [ObservableProperty]
        private string _message = string.Empty;

        public MainViewModel(DatabaseService databaseService, CarSpawningService carSpawningService)
        {
            _databaseService = databaseService;
            _carSpawningService = carSpawningService;

            // Subscribe to spawning events
            _carSpawningService.CarSpawned += OnCarSpawned;
            _carSpawningService.CarLeft += OnCarLeft;
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            try
            {
                Message = "Loading data...";

                ParkingLot = await _databaseService.GetParkingLotAsync();
                Cars = new ObservableCollection<Car>(await _databaseService.GetAllCarsAsync());
                TotalSpaces = ParkingLot?.TotalSpaces ?? 0;
                OccupiedSpaces = ParkingLot?.OccupiedSpaces ?? 0;
                AvailableSpaces = TotalSpaces - OccupiedSpaces;

                Message = "Data successfully loaded";

                
                _carSpawningService.StartSpawning(); // Start automatic spawning
                _carSpawningService.StartRemoving();
            }
            catch (Exception ex)
            {
                Message = $"Failed to load data: {ex.Message}";
            }
        }

        private async void OnCarSpawned(object? sender, Car car)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                Cars.Add(car);
                await RefreshStatsAsync();
                Message = $"New car arrived: {car.Brand} ({car.NumberPlate})";
            });
        }

        private async void OnCarLeft(object? sender, Car car)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var carToRemove = Cars.FirstOrDefault(c => c.ID == car.ID);
                if (carToRemove != null)
                {
                    Cars.Remove(carToRemove);
                }
                await RefreshStatsAsync();
                Message = $"Car left: {car.Brand} ({car.NumberPlate})";
            });
        }

        private async Task RefreshStatsAsync()
        {
            ParkingLot = await _databaseService.GetParkingLotAsync();
            OccupiedSpaces = ParkingLot?.OccupiedSpaces ?? 0;
            AvailableSpaces = TotalSpaces - OccupiedSpaces;
        }
    }
}