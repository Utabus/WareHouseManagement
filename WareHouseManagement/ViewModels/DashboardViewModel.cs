using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using WareHouseManagement.Data;
using WareHouseManagement.Models;

namespace WareHouseManagement.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();
        private DateTime _fromDate;
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                if (_fromDate != value)
                {
                    _fromDate = value;
                    OnPropertyChanged(nameof(FromDate));
                    LoadRevenueData(_fromDate, _toDate);
                }
            }
        }

        private DateTime _toDate;
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                if (_toDate != value)
                {
                    _toDate = value;
                    OnPropertyChanged(nameof(ToDate));
                    LoadRevenueData(_fromDate, _toDate);
                }
            }
        }

        private DashboardSummary _summary;
        public DashboardSummary Summary
        {
            get { return _summary; }
            set
            {
                _summary = value;
                OnPropertyChanged("Summary");
            }
        }

        private SeriesCollection _revenueSeries;
        public SeriesCollection RevenueSeries
        {
            get { return _revenueSeries; }
            set
            {
                _revenueSeries = value;
                OnPropertyChanged("RevenueSeries");
            }
        }

        private string[] _labels;
        public string[] Labels
        {
            get { return _labels; }
            set
            {
                _labels = value;
                OnPropertyChanged("Labels");
            }
        }

        private Func<double, string> _yFormatter;
        public Func<double, string> YFormatter
        {
            get { return _yFormatter; }
            set
            {
                _yFormatter = value;
                OnPropertyChanged("YFormatter");
            }
        }

        private List<int> _years;
        public List<int> Years
        {
            get { return _years; }
            set
            {
                _years = value;
                OnPropertyChanged("Years");
            }
        }



        private float _TotalExportValue;
        public float TotalExportValue
        {
            get { return _TotalExportValue; }
            set
            {
                _TotalExportValue = value;
                OnPropertyChanged("TotalExportValue");
            }
        }
        private float _TotalProfit;
        public float TotalProfit
        {
            get { return _TotalProfit; }
            set
            {
                _TotalProfit = value;
                OnPropertyChanged("TotalProfit");
            }
        }




        //private int _selectedYear;
        //public int SelectedYear
        //{
        //    get { return _selectedYear; }
        //    set
        //    {
        //        if (_selectedYear != value)
        //        {
        //            _selectedYear = value;
        //            OnPropertyChanged("SelectedYear");
        //            LoadRevenueData(_selectedYear);
        //        }
        //    }
        //}

        public DashboardViewModel()
        {
            try
            {
                Summary = _db.GetDashboardSummary();

                Years = new List<int>();
                for (int i = 2020; i <= DateTime.Now.Year; i++)
                    Years.Add(i);

                //SelectedYear = DateTime.Now.Year;

                YFormatter = value => value.ToString("N0") + " đ";

                ToDate = DateTime.Today;
                FromDate = DateTime.Now.AddMonths(-1);
                LoadRevenueData(FromDate, ToDate);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("DashboardViewModel Error: " + ex.Message);
            }
        }


        private void LoadRevenueData(DateTime from, DateTime to)
        {
            var data = _db.GetRevenueByDate(from, to).ToList();

            var allDays = Enumerable.Range(0, (to - from).Days + 2)
                .Select(d => from.AddDays(d))
                .ToList();

            Labels = allDays
                .Select(d => d.ToString("dd/MM"))
                .ToArray();

            double[] revenueValues = allDays
                .Select(d => (double)(data.FirstOrDefault(x => x.InvoiceDate.Date == d.Date)?.TotalRevenue ?? 0))
                .ToArray();

            double[] profitValues = allDays
                .Select(d => (double)(data.FirstOrDefault(x => x.InvoiceDate.Date == d.Date)?.TotalProfit ?? 0))
                .ToArray();
            TotalExportValue = revenueValues.Sum(x => (float)x);    
            TotalProfit = profitValues.Sum(x => (float)x);
            RevenueSeries = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Doanh thu",
            Values = new ChartValues<double>(revenueValues),
            Fill = new SolidColorBrush(Color.FromRgb(63,81,181))
        },
        new LineSeries
        {
            Title = "Lợi nhuận",
            Values = new ChartValues<double>(profitValues),
            Stroke = new SolidColorBrush(Color.FromRgb(233,30,99)),
            Fill = Brushes.Transparent,
            PointGeometrySize = 8
        }
    };
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
