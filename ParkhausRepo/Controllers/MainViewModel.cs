using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParkhausRepo.Services;
using ParkhausRepo.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ParkhausRepo.Entities;

namespace ParkhausRepo.Controllers
{
    internal partial class MainViewModel :ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ParkingLot? _parkingLot;

        [ObservableProperty]
        private ObservableCollection<Car> _cars;

        [ObservableProperty]
        private int _totalSpaces;

        [ObservableProperty]
        private int _availableSpaces;

        [ObservableProperty]
        private int _occupiedSpaces;

        private string _message = "";

        public 
        private async Task LoadDataAsync() 
        {
            try
            {
                _message = "Now loading Data";
                ParkingLot = _databaseService.GetParkingLotAsync().Result;
                Cars = new ObservableCollection<Car>(_databaseService.GetAllCarsAsync().Result);
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
