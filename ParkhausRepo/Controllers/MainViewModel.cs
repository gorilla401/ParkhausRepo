using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParkhausRepo.Services;
using ParkhausRepo.Models;
using ParkhausRepo.Entities;
using System.Collections.ObjectModel;

namespace ParkhausRepo.Controllers
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService = DatabaseService.Instance;

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

        [RelayCommand]
        private async Task LoadAsync()
        {
            try
            {
                _message = "Data successfully loaded";
                ParkingLot = await _databaseService.GetParkingLotAsync();
                Cars = new ObservableCollection<Car>(await _databaseService.GetAllCarsAsync());
                TotalSpaces = ParkingLot?.TotalSpaces ?? 0;
                OccupiedSpaces = ParkingLot?.OccupiedSpaces ?? 0;
                AvailableSpaces = TotalSpaces - OccupiedSpaces;
            }
            catch
            {
                _message = "Failed to load data";
            }
        }
    }
}
