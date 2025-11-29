using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WareHouseManagement.Data;
using WareHouseManagement.Models;

namespace WareHouseManagement.ViewModels
{
    public class ProductEditViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _repo;
        private Product _product;
        private bool _isEdit;

        public Product Product
        {
            get => _product;
            set
            {
                if (_product != value)
                {
                    _product = value;
                    OnPropertyChanged(nameof(Product));
                }
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor thêm mới
        public ObservableCollection<ProductType> ProductTypes { get; set; }

        public ProductEditViewModel()
        {
            _repo = new DatabaseHelper();
            Product = new Product();
            _isEdit = false;
            Title = "Thêm sản phẩm";

            // Load loại sản phẩm từ database
            ProductTypes = new ObservableCollection<ProductType>(_repo.GetProductTypes());

            SaveCommand = new RelayCommand<object>(CanSave, Save);
            CancelCommand = new RelayCommand<object>(_ => true, Cancel);
        }

        // Khi chỉnh sửa
        public ProductEditViewModel(Product p)
        {
            _repo = new DatabaseHelper();
            Product = new Product
            {
                Id = p.Id,
                Series = p.Series,
                ProductName = p.ProductName,
                Color = p.Color,
                Capacity = p.Capacity,
                CostPrice = p.CostPrice,
                SellPrice = p.SellPrice,
                Quantity = p.Quantity,
                ImportDate = p.ImportDate,
                ProductTypeId = p.ProductTypeId
            };
            _isEdit = true;
            Title = "Sửa sản phẩm";

            ProductTypes = new ObservableCollection<ProductType>(_repo.GetProductTypes());

            SaveCommand = new RelayCommand<object>(CanSave, Save);
            CancelCommand = new RelayCommand<object>(_ => true, Cancel);
        }


        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool CanSave(object parameter) => true;

        private void Save(object parameter)
        {
            try
            {
                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(Product.Series))
                {
                    MessageBox.Show("Series không được để trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Product.ProductName))
                {
                    MessageBox.Show("Tên sản phẩm không được để trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Product.CostPrice <= 0)
                {
                    MessageBox.Show("Giá nhập phải lớn hơn 0!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Product.Quantity < 0)
                {
                    MessageBox.Show("Số lượng không được âm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (Product.ProductTypeId < 1)
                {
                    MessageBox.Show("Vui lòng chọn Loại sản phẩm", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Kiểm tra trùng Series hoặc Tên khi thêm mới
                if (!_isEdit && _repo.IsProductExist(Product.Series ))
                {
                    MessageBox.Show("Series đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Kiểm tra trùng khi update, loại bỏ bản đang sửa
                if (_isEdit && _repo.IsProductExist(Product.Series, Product.Id))
                {
                    MessageBox.Show("Series  đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Product.ImportDate = DateTime.Now;

                if (_isEdit)
                    _repo.UpdateProduct(Product);
                else
                    _repo.InsertProduct(Product);

                if (parameter is Window win)
                    win.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
            if (parameter is Window win)
                win.DialogResult = false;
        }
    }
}

