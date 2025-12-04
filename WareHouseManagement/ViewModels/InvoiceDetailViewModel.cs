using System.Collections.ObjectModel;
using System.ComponentModel;
using WareHouseManagement.Data;
using WareHouseManagement.Models;

namespace WareHouseManagement.ViewModels
{
    public class InvoiceDetailViewModel : INotifyPropertyChanged
    {
        public Invoice Invoice { get; }

        private ObservableCollection<InvoiceDetailItem> _items;
        public ObservableCollection<InvoiceDetailItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public InvoiceDetailViewModel(Invoice invoice)
        {
            Invoice = invoice;

            // nếu bạn lưu item trong DB:
            LoadItems();
        }

        private void LoadItems()
        {
            // gọi DB theo invoice.Id
            var db = new DatabaseHelper();
            var data = db.GetInvoiceItems(Invoice.Id);

            Items = new ObservableCollection<InvoiceDetailItem>(data);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
