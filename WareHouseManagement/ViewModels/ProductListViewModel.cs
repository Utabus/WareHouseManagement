using CommunityToolkit.Mvvm.Input;
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
        public List<ProductType> ProductTypes { get; set; }
        public ObservableCollection<Product> Products { get; set; }

        private string _keyword;
        public string Keyword
        {
            get => _keyword;
            set { _keyword = value; OnPropertyChanged(); }
        }

        public ICommand SearchCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public ProductListViewModel()
        {
            _repo = new DatabaseHelper();
            Products = new ObservableCollection<Product>(_repo.GetProducts());
            ProductTypes = _repo.GetProductTypes().ToList();

            SearchCommand = new RelayCommand<object>((_) => true, (_) => Search());
            AddCommand = new RelayCommand<object>((_) => true, (_) => AddProduct());
            EditCommand = new RelayCommand<Product>((p) => p != null, (p) => EditProduct(p));
            DeleteCommand = new RelayCommand<Product>((p) => p != null, (p) => DeleteProduct(p));
            RefreshCommand = new RelayCommand<object>((_) => true, (_) => LoadProducts());
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
            var filtered = list.Where(p =>
                string.IsNullOrEmpty(Keyword)
                || p.ProductName.ToLower().Contains(Keyword.ToLower())
                || p.Series.ToLower().Contains(Keyword.ToLower())).ToList();

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
            if (MessageBox.Show($"Bạn có chắc muốn xóa sản phẩm '{p.ProductName}'?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _repo.DeleteProduct(p.Id);
                LoadProducts();
            }
        }
    }
}
