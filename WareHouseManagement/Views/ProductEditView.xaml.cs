using System.Windows;
using WareHouseManagement.Models;
using WareHouseManagement.ViewModels;

namespace WareHouseManagement.Views
{
    /// <summary>
    /// Interaction logic for ProductEditView.xaml
    /// </summary>
    public partial class ProductEditView : Window
    {
        // Constructor thêm mới
        public ProductEditView()
        {
            InitializeComponent();
            DataContext = new ProductEditViewModel
            {
                Title = "Thêm Sản Phẩm"
            };
        }

        // Constructor chỉnh sửa, truyền vào Product cần sửa
        public ProductEditView(ProductEditViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
