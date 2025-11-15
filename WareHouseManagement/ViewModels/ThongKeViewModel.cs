using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
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

        private ObservableCollection<InvoiceProductInfo> _InvoiceProductInfo = new ObservableCollection<InvoiceProductInfo>();
        public ObservableCollection<InvoiceProductInfo> InvoiceProductInfo
        {
            get => _InvoiceProductInfo;
            set
            {
                _InvoiceProductInfo = value;
                OnPropertyChanged(nameof(InvoiceProductInfo));
                OnPropertyChanged(nameof(TotalRevenue));
                OnPropertyChanged(nameof(TotalProfit));
            }
        }

        public decimal TotalRevenue => InvoiceProductInfo?.Sum(x => x.SellPrice) ?? 0;
        public decimal TotalProfit => InvoiceProductInfo?.Sum(x => x.SellPrice - x.CostPrice) ?? 0;


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
            var data = _db.GetInvoiceProducts();
            InvoiceProductInfo = new ObservableCollection<InvoiceProductInfo>(data);
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
