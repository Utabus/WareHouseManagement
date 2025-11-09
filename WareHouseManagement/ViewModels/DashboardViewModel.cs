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

        private int _selectedYear;
        public int SelectedYear
        {
            get { return _selectedYear; }
            set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                    OnPropertyChanged("SelectedYear");
                    LoadRevenueData(_selectedYear);
                }
            }
        }

        public DashboardViewModel()
        {
            try
            {
                Summary = _db.GetDashboardSummary();

                Years = new List<int>();
                for (int i = 2020; i <= DateTime.Now.Year; i++)
                    Years.Add(i);

                SelectedYear = DateTime.Now.Year;

                YFormatter = value => value.ToString("N0") + " đ";

                LoadRevenueData(SelectedYear);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("DashboardViewModel Error: " + ex.Message);
            }
        }


        private void LoadRevenueData(int year)
        {
            var data = _db.GetRevenueByMonth(year).ToList();

            string[] months = Enumerable.Range(1, 12).Select(m => m.ToString("00")).ToArray();
            Labels = months.Select(m => "T" + m).ToArray(); // nhãn hiển thị T01, T02...


            double[] revenueValues = months
      .Select(m => (double)(data.FirstOrDefault(x => x.Month == m)?.TotalRevenue ?? 0))
      .ToArray();

            double[] profitValues = months
                .Select(m => (double)(data.FirstOrDefault(x => x.Month == m)?.TotalProfit ?? 0))
                .ToArray();

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
