using System.Windows.Controls;
using WareHouseManagement.ViewModels;

namespace WareHouseManagement.Views
{
    /// <summary>
    /// Interaction logic for HoaDonUC.xaml
    /// </summary>
    public partial class HoaDonUC : UserControl
    {
        public HoaDonUC()
        {
            InitializeComponent();
            DataContext = new HoaDonViewModel();
        }
    }
}
