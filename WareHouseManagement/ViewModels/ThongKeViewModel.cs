using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WareHouseManagement.Data;
using WareHouseManagement.Models;

namespace WareHouseManagement.ViewModels
{
    public class ThongKeViewModel : ObservableObject
    {
        private readonly DatabaseHelper _db;

        public ThongKeViewModel()
        {
            _db = new DatabaseHelper();

            Years = new ObservableCollection<int>(Enumerable.Range(DateTime.Now.Year - 5, 6));
            SelectedYear = DateTime.Now.Year;

            LoadRevenueStatisticsCommand = new RelayCommand(o => LoadRevenueStatistics());
            ExportExcelCommand = new RelayCommand(o => ExportExcel());
            ExportPdfCommand = new RelayCommand(o => ExportPdf());

            LoadRevenueStatistics();
        }

        // ------------------------------
        // Properties
        // ------------------------------
        public ObservableCollection<int> Years { get; set; }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                    OnPropertyChanged(nameof(SelectedYear));
                    LoadRevenueStatistics();
                }
            }
        }

        private ObservableCollection<RevenueStatistic> _statistics = new ObservableCollection<RevenueStatistic>();
        public ObservableCollection<RevenueStatistic> Statistics
        {
            get => _statistics;
            set
            {
                _statistics = value;
                OnPropertyChanged(nameof(Statistics));
                OnPropertyChanged(nameof(TotalRevenue));
                OnPropertyChanged(nameof(TotalProfit));
            }
        }

        public decimal TotalRevenue => Statistics?.Sum(x => x.TotalRevenue) ?? 0;
        public decimal TotalProfit => Statistics?.Sum(x => x.TotalProfit) ?? 0;

        // ------------------------------
        // Commands
        // ------------------------------
        public ICommand LoadRevenueStatisticsCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand ExportPdfCommand { get; }

        // ------------------------------
        // Methods
        // ------------------------------
        private void LoadRevenueStatistics()
        {
            var data = _db.GetRevenueByMonth(SelectedYear);
            Statistics = new ObservableCollection<RevenueStatistic>(data);
        }

        private void ExportExcel()
        {
            // TODO: Xuất Excel
        }

        private void ExportPdf()
        {
            // TODO: Xuất PDF
        }
    }

    // ------------------------------
    // Base class ObservableObject
    // ------------------------------
   
}
