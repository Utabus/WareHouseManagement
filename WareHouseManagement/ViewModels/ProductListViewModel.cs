using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WareHouseManagement.Data;
using WareHouseManagement.Models;
using WareHouseManagement.Views;
namespace WareHouseManagement.ViewModels
{
    public class ProductListViewModel : BaseViewModel
    {
        private readonly DatabaseHelper _repo;
        public List<ProductType> ProductTypes { get; set; } = new List<ProductType>();
        public ObservableCollection<Product> Products { get; set; }

        private string _keyword;
        public string Keyword
        {
            get => _keyword;
            set { _keyword = value; OnPropertyChanged(); }
        }
        private ProductType _selectedProductType;
        public ProductType SelectedProductType
        {
            get => _selectedProductType;
            set
            {
                _selectedProductType = value;
                OnPropertyChanged(nameof(SelectedProductType));
            }
        }


        public ICommand SearchCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }

        public ProductListViewModel()
        {
            _repo = new DatabaseHelper();
            Products = new ObservableCollection<Product>(_repo.GetProducts());
            ProductTypes.Insert(0, new ProductType { Id = 0, TypeName = "Chọn Loại sản phẩm" });
            ProductTypes.AddRange(_repo.GetProductTypes().ToList());
            SelectedProductType = ProductTypes.FirstOrDefault();
            SearchCommand = new RelayCommand<object>((_) => true, (_) => Search());
            AddCommand = new RelayCommand<object>((_) => true, (_) => AddProduct());
            EditCommand = new RelayCommand<Product>((p) => p != null, (p) => EditProduct(p));
            DeleteCommand = new RelayCommand<Product>((p) => p != null, (p) => DeleteProduct(p));
            RefreshCommand = new RelayCommand<object>((_) => true, (_) => LoadProducts());
            ExportCommand = new RelayCommand(ExportProducts);
            ImportCommand = new RelayCommand(ImportProducts);
        }

        private void ExportProducts(object obj)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "Products.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
                _repo.ExportProductsToExcel(dlg.FileName);
                HandyControl.Controls.MessageBox.Info("✅ Export thành công!");
            }
        }

        private void ImportProducts(object obj)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
                _repo.ImportProductsFromExcel(dlg.FileName);
                LoadProducts();
                HandyControl.Controls.MessageBox.Info("✅ Import thành công!");
            }
        }

        public void LoadProducts()
        {
            Products.Clear();
            foreach (var item in _repo.GetProducts())
                Products.Add(item);
        }

        public void Search()
        {
            var list = _repo.GetProducts();

            // Nếu ProductTypeId = 0 → tìm tất cả
            var filtered = list.Where(p =>
                (SelectedProductType.Id == 0 || p.ProductTypeId == SelectedProductType.Id) &&
                (string.IsNullOrEmpty(Keyword)
                 || p.ProductName.IndexOf(Keyword, StringComparison.OrdinalIgnoreCase) >= 0
                 || p.Series.IndexOf(Keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            Products.Clear();
            foreach (var item in filtered)
                Products.Add(item);
        }

        public void AddProduct()
        {
            var win = new ProductEditView(); // Thêm mới
            var vm = new ProductEditViewModel();
            win.DataContext = vm;
            if (win.ShowDialog() == true)
            {

                LoadProducts();
            }
        }

        public void EditProduct(Product p)
        {
            var win = new ProductEditView(); // Chỉnh sửa
            var vm = new ProductEditViewModel(p);
            win.DataContext = vm;
            if (win.ShowDialog() == true)
            {

                LoadProducts();
            }
        }


        public void DeleteProduct(Product p)
        {
            if (_repo.IsProductUsed(p.Id))
            {
                HandyControl.Controls.MessageBox.Error("❌ Sản phẩm đang nằm trong hóa đơn, không thể xóa.");
                return;
            }

            if (MessageBox.Show($"Bạn có chắc muốn xóa sản phẩm '{p.ProductName}'?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _repo.DeleteProduct(p.Id);
                LoadProducts();
                HandyControl.Controls.MessageBox.Success("🗑️ Xóa thành công!");
            }
        }

    }
}
