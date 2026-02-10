using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParkhausRepo.Entities;
using ParkhausRepo.Models;
using ParkhausRepo.Services;
using System.Collections.ObjectModel;

namespace ParkhausRepo.Controllers
{
    public partial class MapViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService = DatabaseService.Instance;

        [ObservableProperty]
        private ParkingLot? _parkingLot ;

        [ObservableProperty]
        private ObservableCollection<ParkingSpace> _parkingSpaces = new();

        [RelayCommand]
        private async Task LoadAsync()
        {
            ParkingLot = await _databaseService.GetParkingLotAsync();
            ParkingSpaces = new ObservableCollection<ParkingSpace>(await _databaseService.GetAllParkingSpacesAsync());
        }
    }
}
