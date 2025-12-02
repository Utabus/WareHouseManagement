using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WareHouseManagement.Data;
using WareHouseManagement.Models;

namespace WareHouseManagement.ViewModels.BanHang
{
    public class BanHangViewModel : ObservableObject
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        public ObservableCollection<Product> DanhSachSanPham { get; set; }
        public ObservableCollection<InvoiceItemView> GioHang { get; set; }

        private decimal tongTien;
        public decimal TongTien
        {
            get => tongTien;
            private set => SetProperty(ref tongTien, value);
        }

        private decimal loiNhuan;
        public decimal LoiNhuan
        {
            get => loiNhuan;
            private set => SetProperty(ref loiNhuan, value);
        }

        private string customerName;
        public string CustomerName
        {
            get => customerName;
            set => SetProperty(ref customerName, value);
        }

        // Hàm cập nhật lại tổng tiền & lợi nhuận
        private void CapNhatTongTienLoiNhuan()
        {
            TongTien = GioHang.Sum(x => x.Total);
            LoiNhuan = GioHang.Sum(x => (x.UnitPrice - x.CostPrice) * x.Quantity);
        }
        private bool isDebt; // true = nợ, false = đã thanh toán
        public bool IsDebt
        {
            get => isDebt;
            set => SetProperty(ref isDebt, value);
        }

        private string tuKhoaTimKiem;
        public string TuKhoaTimKiem
        {
            get => tuKhoaTimKiem;
            set
            {
                if (SetProperty(ref tuKhoaTimKiem, value))
                    LocSanPham();
            }
        }

        public ICommand ThemVaoGioCommand { get; }
        public ICommand XoaKhoiGioCommand { get; }
        public ICommand LuuHoaDonCommand { get; }
        public ICommand TaiLaiCommand { get; }

        public BanHangViewModel()
        {
            DanhSachSanPham = new ObservableCollection<Product>(_db.GetProducts().Where(p => p.Quantity > 0));
            GioHang = new ObservableCollection<InvoiceItemView>();
            // subscribe khi item thay đổi quantity
            GioHang.CollectionChanged += GioHang_CollectionChanged;
            // predicate (canExecute) trước, action (execute) sau
            ThemVaoGioCommand = new RelayCommand<Product>(p => p != null, ThemVaoGio);
            XoaKhoiGioCommand = new RelayCommand<InvoiceItemView>(item => item != null, XoaKhoiGio);
            LuuHoaDonCommand = new RelayCommand<object>(null, LuuHoaDon);

            TaiLaiCommand = new RelayCommand<object>(null, TaiLai);
        }
        private void LocSanPham()
        {
            if (string.IsNullOrWhiteSpace(TuKhoaTimKiem))
            {
                DanhSachSanPham = new ObservableCollection<Product>(_db.GetProducts().Where(p => p.Quantity > 0));
            }
            else
            {
                string keyword = TuKhoaTimKiem.ToLower();
                DanhSachSanPham = new ObservableCollection<Product>(
                    _db.GetProducts()
                    .Where(p => p.Quantity > 0 &&
                                (p.Series.ToLower().Contains(keyword) ||
                                 p.ProductName.ToLower().Contains(keyword)))
                );
            }

            OnPropertyChanged(nameof(DanhSachSanPham));
        }

        private void GioHang_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (InvoiceItemView item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (InvoiceItemView item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }

            // cập nhật tổng khi có item mới hoặc bị xóa
            CapNhatTongTienLoiNhuan();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceItemView.Total) || e.PropertyName == nameof(InvoiceItemView.Quantity))
            {
                CapNhatTongTienLoiNhuan();
            }
        }

        private void ThemVaoGio(Product sp)
        {
            if (sp == null) return;

            var existing = GioHang.FirstOrDefault(x => x.ProductId == sp.Id);
            if (existing != null)
            {
                if (existing.Quantity < sp.Quantity)
                    existing.Quantity++;
                else
                    MessageBox.Show("Không đủ hàng tồn!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                GioHang.Add(new InvoiceItemView
                {
                    ProductId = sp.Id,
                    ProductName = sp.ProductName,
                    CostPrice = sp.CostPrice,
                    UnitPrice = sp.SellPrice,
                    Quantity = 1,
                    //Total = sp.SellPrice 
                });
            }
            CapNhatTongTienLoiNhuan();
        }

        private void XoaKhoiGio(InvoiceItemView item)
        {
            if (item == null) return;
            GioHang.Remove(item);
            CapNhatTongTienLoiNhuan();
        }

        private void LuuHoaDon(object obj)
        {
            if (!GioHang.Any())
            {
                MessageBox.Show("Chưa có sản phẩm nào trong hóa đơn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (string.IsNullOrWhiteSpace(this.CustomerName))
            {
                MessageBox.Show("Bạn chưa nhập tên khách hàng.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var invoice = new Invoice
                {
                    InvoiceCode = "HDX" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    InvoiceDate = DateTime.Now,
                    Type = "Export",
                    CustomerName = this.CustomerName,
                    TotalAmount = TongTien,
                    Profit = LoiNhuan,
                    IsDebt = this.IsDebt // bind trực tiếp từ ComboBox
                };


                var details = GioHang.Select(x => new InvoiceDetail
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    Total = x.Total
                }).ToList();

                _db.InsertInvoice(invoice, details);

                MessageBox.Show("Đã lưu hóa đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                TaiLai(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaiLai(object obj)
        {
            GioHang.Clear();
            DanhSachSanPham = new ObservableCollection<Product>(_db.GetProducts().Where(p => p.Quantity > 0));
            OnPropertyChanged(nameof(DanhSachSanPham));
            CapNhatTongTienLoiNhuan();
        }

    }

}

public class InvoiceItemView : ObservableObject
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal UnitPrice { get; set; }

    private int quantity;
    public int Quantity
    {
        get => quantity;
        set
        {
            if (SetProperty(ref quantity, value))
            {
                OnPropertyChanged(nameof(Total));
            }
        }
    }
    public decimal Total => UnitPrice * Quantity;

}