using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParkhausRepo.Models;
using ParkhausRepo.Services;
using System.Collections.ObjectModel;

namespace ParkhausRepo.Controllers
{
    public partial class MapViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly CarSpawningService _carSpawningService;
        private List<ParkingSpace> _allParkingSpaces = new();
        private List<Car> _allCars = new();

        [ObservableProperty]
        private ParkingLot? _parkingLot;

        [ObservableProperty]
        private ObservableCollection<ParkingSpaceDisplay> _currentFloorSpaces = new();

        [ObservableProperty]
        private int _currentFloor = 1;

        [ObservableProperty]
        private int _totalFloors = 4;

        [ObservableProperty]
        private bool _canGoDown = false;

        [ObservableProperty]
        private bool _canGoUp = true;

        [ObservableProperty]
        private float _totalEarnings = 0f;

        public MapViewModel(DatabaseService databaseService, CarSpawningService carSpawningService)
        {
            _databaseService = databaseService;
            _carSpawningService = carSpawningService;

            // Subscribe to car events for real-time updates
            _carSpawningService.CarSpawned += OnCarEvent;
            _carSpawningService.CarLeft += OnCarEvent;
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            try
            {
                ParkingLot = await _databaseService.GetParkingLotAsync();
                TotalFloors = ParkingLot?.Floors ?? 4;
                TotalEarnings = ParkingLot?.TotalEarnings ?? 0f;
                _allParkingSpaces = await _databaseService.GetAllParkingSpacesAsync();
                _allCars = await _databaseService.GetAllCarsAsync();

                UpdateCurrentFloor();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading parking spaces: {ex.Message}");
            }
        }

        [RelayCommand]
        private void NextFloor()
        {
            if (CurrentFloor < TotalFloors)
            {
                CurrentFloor++;
                UpdateCurrentFloor();
            }
        }

        [RelayCommand]
        private void PreviousFloor()
        {
            if (CurrentFloor > 1)
            {
                CurrentFloor--;
                UpdateCurrentFloor();
            }
        }

        private void UpdateCurrentFloor()
        {
            var floorSpaces = _allParkingSpaces
                .Where(s => s.SpaceFloor == CurrentFloor)
                .OrderBy(s => s.SpaceNumber)
                .ToList();

            CurrentFloorSpaces.Clear();

            foreach (var space in floorSpaces)
            {
                var car = space.IsOccupied && space.CurrentCarID.HasValue
                    ? _allCars.FirstOrDefault(c => c.ID == space.CurrentCarID.Value)
                    : null;

                CurrentFloorSpaces.Add(new ParkingSpaceDisplay
                {
                    Space = space,
                    Car = car,
                    IsOccupied = space.IsOccupied,
                    DisplayText = space.IsOccupied && car != null
                        ? $"{car.Brand}\n{car.NumberPlate}"
                        : $"Space {space.SpaceNumber}",
                    BackgroundColor = space.IsOccupied ? "#FF6B6B" : "#51CF66"
                });
            }

            CanGoDown = CurrentFloor > 1;
            CanGoUp = CurrentFloor < TotalFloors;
        }

        private async void OnCarEvent(object? sender, Car car)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                ParkingLot = await _databaseService.GetParkingLotAsync();
                TotalEarnings = ParkingLot?.TotalEarnings ?? 0f;
                _allParkingSpaces = await _databaseService.GetAllParkingSpacesAsync();
                _allCars = await _databaseService.GetAllCarsAsync();
                UpdateCurrentFloor();
            });
        }
    }

    public class ParkingSpaceDisplay
    {
        public ParkingSpace Space { get; set; } = new();
        public Car? Car { get; set; }
        public bool IsOccupied { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
    }
}