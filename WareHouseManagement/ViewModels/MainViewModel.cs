using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using WareHouseManagement.Views;

namespace WareHouseManagement.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand NavigateCommand { get; }

        private IServiceProvider serviceProvider;
        public MainViewModel(IServiceProvider serviceProvider)
        {
            CurrentView = new Views.DashboardUC();
            this.serviceProvider = serviceProvider;
            // Khởi tạo RelayCommand đúng cách
            NavigateCommand = new RelayCommand<string>(
      null,   // canExecute
      Navigate // execute
  );

        }

        private void Navigate(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                return;

            // Sử dụng if-else để tương thích C# 7.3
            if (viewName == "Dashboard")
                CurrentView = serviceProvider.GetRequiredService<DashboardUC>();
            else if (viewName == "NhapHang")
                CurrentView = serviceProvider.GetRequiredService<ProductListView>();
            else if (viewName == "LoaiSanPham")
                CurrentView = serviceProvider.GetRequiredService<ProductTypeUC>();
            else if (viewName == "BanHang")
                CurrentView = new Views.BanHangUC();
            else if (viewName == "HoaDon")
                CurrentView = new Views.HoaDonUC();
            else if (viewName == "ThongKe")
                CurrentView = new Views.ThongKeUC();
            // else giữ nguyên CurrentView
        }
    }
}
