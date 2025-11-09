using System.Windows;
using WareHouseManagement.ViewModels;

namespace WareHouseManagement.Views
{
    /// <summary>
    /// Interaction logic for ProductTypeEditView.xaml
    /// </summary>
    public partial class ProductTypeEditView : Window
    {
        public ProductTypeEditView()
        {
            InitializeComponent();

            // Nếu muốn thêm mới
            this.DataContext = new ProductTypeEditViewModel
            {
                Title = "Thêm Loại Sản Phẩm"
            };
        }

        // Nếu muốn tạo window cho chỉnh sửa
        public ProductTypeEditView(ProductTypeEditViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
