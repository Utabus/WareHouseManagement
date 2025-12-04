using System.Windows.Controls;
using System.Windows.Input;
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


        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is HoaDonViewModel vm && vm.SelectedInvoice != null)
            {
                vm.OpenInvoiceDetailCommand.Execute(null);
            }
        }

    }
}
