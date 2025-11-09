using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WareHouseManagement.Data;
using WareHouseManagement.Models;

namespace WareHouseManagement.ViewModels
{
    public class ProductTypeEditViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _repo;
        private ProductType _productType;
        private bool _isEdit;
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

        public ProductType ProductType
        {
            get => _productType;
            set
            {
                if (_productType != value)
                {
                    _productType = value;
                    OnPropertyChanged(nameof(ProductType));
                }
            }
        }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ProductTypeEditViewModel()
        {
            Title = "Thêm Loại Sản Phẩm";
            _repo = new DatabaseHelper();
            ProductType = new ProductType();
            ProductType.TypeCode = Guid.NewGuid().ToString();

            _isEdit = false;
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        public ProductTypeEditViewModel(ProductType pt)
        {
            Title = "Sửa Loại Sản Phẩm";
            _repo = new DatabaseHelper();
            ProductType = new ProductType
            {
                Id = pt.Id,
                TypeCode = pt.TypeCode,
                TypeName = pt.TypeName,
                Description = pt.Description
            };
            _isEdit = true;
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool CanSave(object parameter) => true;


        private void Save(object parameter)
        {
            if (string.IsNullOrWhiteSpace(ProductType.TypeCode) ||
                string.IsNullOrWhiteSpace(ProductType.TypeName))
            {
                MessageBox.Show("Mã loại và Tên loại không được để trống!");
                return;
            }

            try
            {
                // Kiểm tra trùng khi thêm mới
                if (!_isEdit && _repo.IsProductTypeExist(ProductType.TypeCode, ProductType.TypeName))
                {
                    MessageBox.Show("Mã loại hoặc Tên loại đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Kiểm tra trùng khi update
                if (_isEdit && _repo.IsProductTypeExist(ProductType.TypeCode, ProductType.TypeName, ProductType.Id))
                {
                    MessageBox.Show("Mã loại hoặc Tên loại đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_isEdit)
                    _repo.UpdateProductType(ProductType);
                else
                    _repo.InsertProductType(ProductType);

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
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public event EventHandler CanExecuteChanged;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

