using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WareHouseManagement.Data;
using WareHouseManagement.Models;
using WareHouseManagement.Views;

namespace WareHouseManagement.ViewModels
{
    public class ProductTypeUCViewModel : BaseViewModel
    {
        private readonly DatabaseHelper _repo;
        public ObservableCollection<ProductType> ProductTypes { get; set; }

        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public ProductTypeUCViewModel()
        {
            _repo = new DatabaseHelper();
            ProductTypes = new ObservableCollection<ProductType>(_repo.GetProductTypes());

            AddCommand = new RelayCommand<object>((_) => true, (_) => Add());
            EditCommand = new RelayCommand<ProductType>((pt) => pt != null, (pt) => Edit(pt));
            DeleteCommand = new RelayCommand<ProductType>((pt) => pt != null, (pt) => Delete(pt));
            RefreshCommand = new RelayCommand<object>((_) => true, (_) => Refresh());
        }

        private void Add()
        {
            var win = new ProductTypeEditView(); // Tạo window edit/add loại
            var vm = new ProductTypeEditViewModel();
            win.DataContext = vm;
            if (win.ShowDialog() == true)
                Refresh();
        }

        private void Edit(ProductType pt)
        {
            var win = new ProductTypeEditView();
            var vm = new ProductTypeEditViewModel(pt);
            win.DataContext = vm;
            if (win.ShowDialog() == true)
                Refresh();
        }

        private void Delete(ProductType pt)
        {
            if (_repo.IsProductTypeUsed(pt.Id))
            {
                MessageBox.Show($"Loại {pt.TypeName} đang được sử dụng bởi sản phẩm khác, không thể xóa!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn có muốn xóa loại {pt.TypeName}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _repo.DeleteProductType(pt.Id);
                Refresh();
            }
        }


        private void Refresh()
        {
            ProductTypes.Clear();
            foreach (var pt in _repo.GetProductTypes())
                ProductTypes.Add(pt);
        }
    }
}
