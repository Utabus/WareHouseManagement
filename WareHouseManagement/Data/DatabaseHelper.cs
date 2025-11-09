using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using WareHouseManagement.Models;

namespace WareHouseManagement.Data
{
    public class DatabaseHelper
    {
        private readonly string _dbPath = "Warehouse.db";
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            _connectionString = $"Data Source={_dbPath};Version=3;";
            InitializeDatabase();
        }
        public void ImportSampleData()
        {
            var conn = CreateConnection();
            conn.Open();

            // -------------------------
            // 1️⃣ Loại sản phẩm  
            // -------------------------
            var types = new List<ProductType>
    {
        new ProductType { TypeCode = "ELEC", TypeName = "Điện tử", Description = "Thiết bị điện tử" },
        new ProductType { TypeCode = "HOME", TypeName = "Gia dụng", Description = "Thiết bị gia dụng" },
        new ProductType { TypeCode = "FASH", TypeName = "Thời trang", Description = "Quần áo, phụ kiện" }
    };
            foreach (var t in types)
            {
                if (!IsProductTypeExist(t.TypeCode, t.TypeName))
                    InsertProductType(t);
            }

            // -------------------------
            // 2️⃣ Sản phẩm
            // -------------------------
            var products = new List<Product>
    {
        new Product { Series="ELEC001", ProductName="Điện thoại A", Color="Đen", Capacity="128GB", CostPrice=3000000, SellPrice=5000000, Quantity=50, ImportDate=DateTime.Now.AddDays(-60), IsSold=false, ProductTypeId=1 },
        new Product { Series="ELEC002", ProductName="Laptop B", Color="Xám", Capacity="256GB", CostPrice=10000000, SellPrice=15000000, Quantity=30, ImportDate=DateTime.Now.AddDays(-45), IsSold=false, ProductTypeId=1 },
        new Product { Series="HOME001", ProductName="Máy hút bụi", Color="Trắng", Capacity="2000W", CostPrice=2000000, SellPrice=3500000, Quantity=40, ImportDate=DateTime.Now.AddDays(-30), IsSold=false, ProductTypeId=2 },
        new Product { Series="FASH001", ProductName="Áo thun", Color="Đỏ", Capacity="M", CostPrice=50000, SellPrice=150000, Quantity=100, ImportDate=DateTime.Now.AddDays(-20), IsSold=false, ProductTypeId=3 },
        new Product { Series="FASH002", ProductName="Quần jean", Color="Xanh", Capacity="L", CostPrice=150000, SellPrice=350000, Quantity=80, ImportDate=DateTime.Now.AddDays(-15), IsSold=false, ProductTypeId=3 }
    };
            foreach (var p in products)
            {
                if (!IsProductExist(p.Series, p.ProductName))
                    InsertProduct(p);
            }

            // -------------------------
            // 3️⃣ Hóa đơn
            // -------------------------
            var invoices = new List<Invoice>
    {
        new Invoice { InvoiceCode="HDX20251101", InvoiceDate=DateTime.Now.AddDays(-10), Type="Export", TotalAmount=0, Profit=0 },
        new Invoice { InvoiceCode="HDX20251102", InvoiceDate=DateTime.Now.AddDays(-9), Type="Export", TotalAmount=0, Profit=0 },
        new Invoice { InvoiceCode="HDX20251103", InvoiceDate=DateTime.Now.AddDays(-8), Type="Export", TotalAmount=0, Profit=0 },
        new Invoice { InvoiceCode="HDX20251025", InvoiceDate=DateTime.Now.AddMonths(-1).AddDays(-5), Type="Export", TotalAmount=0, Profit=0 },
        new Invoice { InvoiceCode="HDX20250915", InvoiceDate=DateTime.Now.AddMonths(-2).AddDays(-3), Type="Export", TotalAmount=0, Profit=0 }
    };

            // -------------------------
            // 4️⃣ Chi tiết hóa đơn (Export)
            // -------------------------
            var invoiceDetails = new List<InvoiceDetail>
    {
        new InvoiceDetail { ProductId=1, Quantity=5, UnitPrice=5000000, Total=5*5000000 },
        new InvoiceDetail { ProductId=2, Quantity=2, UnitPrice=15000000, Total=2*15000000 },
        new InvoiceDetail { ProductId=3, Quantity=3, UnitPrice=3500000, Total=3*3500000 },
        new InvoiceDetail { ProductId=4, Quantity=10, UnitPrice=150000, Total=10*150000 },
        new InvoiceDetail { ProductId=5, Quantity=5, UnitPrice=350000, Total=5*350000 }
    };

            // -------------------------
            // 5️⃣ Thêm hóa đơn và cập nhật tồn kho
            // -------------------------
            foreach (var inv in invoices)
            {
                // Chọn ngẫu nhiên vài sản phẩm cho mỗi hóa đơn
                var rnd = new Random();
                var details = invoiceDetails.OrderBy(x => rnd.Next()).Take(rnd.Next(1, 4)).ToList();

                // Tính TotalAmount và Profit
                inv.TotalAmount = details.Sum(d => d.Total);
                inv.Profit = details.Sum(d =>
                {
                    var product = products.FirstOrDefault(p => p.Id == d.ProductId);
                    if (product == null) return 0; // bỏ qua nếu không tìm thấy
                    return (d.UnitPrice - product.CostPrice) * d.Quantity;
                });

                InsertInvoice(inv, details);
            }

            Console.WriteLine("✅ Import sample data hoàn tất!");
        }

        private IDbConnection CreateConnection() => new SQLiteConnection(_connectionString);

        // =========================================
        // 1️⃣ Tạo database và bảng nếu chưa có
        // =========================================
        private void InitializeDatabase()
        {
            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);

            using (var conn = CreateConnection())
            {
                conn.Open();

                string script = @"
                CREATE TABLE IF NOT EXISTS ProductType (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TypeCode TEXT NOT NULL UNIQUE,
                    TypeName TEXT NOT NULL,
                    Description TEXT
                );

                CREATE TABLE IF NOT EXISTS Product (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Series TEXT NOT NULL ,
                    ProductName TEXT NOT NULL,
                    Color TEXT,
                    Capacity TEXT,
                    CostPrice REAL NOT NULL,
                    SellPrice REAL,
                    Quantity INTEGER DEFAULT 0,
                    ImportDate TEXT,
                    IsSold INTEGER DEFAULT 0,
                    ProductTypeId INTEGER,
                    FOREIGN KEY (ProductTypeId) REFERENCES ProductType(Id)
                );

                CREATE TABLE IF NOT EXISTS Invoice (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceCode TEXT NOT NULL UNIQUE,
                    InvoiceDate TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    TotalAmount REAL DEFAULT 0,
                    Profit REAL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS InvoiceDetail (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    Total REAL NOT NULL,
                    FOREIGN KEY (InvoiceId) REFERENCES Invoice(Id),
                    FOREIGN KEY (ProductId) REFERENCES Product(Id)
                );
                ";
                conn.Execute(script);
            }
        }

        // =========================================
        // 2️⃣ CRUD: ProductType
        // =========================================
        public IEnumerable<ProductType> GetProductTypes()
        {
             var conn = CreateConnection();
            return conn.Query<ProductType>("SELECT * FROM ProductType ORDER BY TypeName");
        }

        public void InsertProductType(ProductType type)
        {
             var conn = CreateConnection();
            conn.Execute("INSERT INTO ProductType (TypeCode, TypeName, Description) VALUES (@TypeCode, @TypeName, @Description)", type);
        }

        public void UpdateProductType(ProductType type)
        {
            var conn = CreateConnection();
            conn.Execute("UPDATE ProductType SET TypeName=@TypeName, Description=@Description WHERE Id=@Id", type);
        }

        public void DeleteProductType(int id)
        {
                var conn = CreateConnection();
            conn.Execute("DELETE FROM ProductType WHERE Id=@id", new { id });
        }
        public bool IsProductTypeExist(string typeCode, string typeName, int excludeId = 0)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM ProductType 
                       WHERE (TypeCode = @TypeCode OR TypeName = @TypeName)
                         AND (@ExcludeId = 0 OR Id <> @ExcludeId)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TypeCode", typeCode);
                    cmd.Parameters.AddWithValue("@TypeName", typeName);
                    cmd.Parameters.AddWithValue("@ExcludeId", excludeId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
        public bool IsProductTypeUsed(int productTypeId)
        {
            using (var conn = CreateConnection())
            {
                string sql = "SELECT COUNT(*) FROM Product WHERE ProductTypeId = @Id";
                return conn.ExecuteScalar<int>(sql, new { Id = productTypeId }) > 0;
            }
        }

        // =========================================
        // 3️⃣ CRUD: Product
        // =========================================
        public IEnumerable<Product> GetProducts()
        {
                         var conn = CreateConnection();
            string sql = @"SELECT p.*, pt.TypeName AS ProductTypeName 
                           FROM Product p 
                           LEFT JOIN ProductType pt ON p.ProductTypeId = pt.Id
                           ORDER BY p.Id DESC";
            return conn.Query<Product>(sql);
        }

        public void InsertProduct(Product p)
        {
            var conn = CreateConnection();
            string sql = @"INSERT INTO Product (Series, ProductName, Color, Capacity, CostPrice, SellPrice, Quantity, ImportDate, IsSold, ProductTypeId)
                           VALUES (@Series, @ProductName, @Color, @Capacity, @CostPrice, @SellPrice, @Quantity, @ImportDate, @IsSold, @ProductTypeId)";
            conn.Execute(sql, p);
        }

        public void UpdateProduct(Product p)
        {
            var conn = CreateConnection();
            string sql = @"UPDATE Product SET 
                            ProductName=@ProductName, Color=@Color, Capacity=@Capacity,
                            CostPrice=@CostPrice, SellPrice=@SellPrice, Quantity=@Quantity, 
                            IsSold=@IsSold, ProductTypeId=@ProductTypeId 
                           WHERE Series=@Series";
            conn.Execute(sql, p);
        }

        public void DeleteProduct(int id)
        {
             var conn = CreateConnection();
            conn.Execute("DELETE FROM Product WHERE Id=@id", new { id });
        }
        public bool IsProductExist(string series, string productName, int excludeId = 0)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM Product 
                       WHERE (Series = @Series OR ProductName = @ProductName)
                         AND (@ExcludeId = 0 OR Id <> @ExcludeId)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Series", series);
                    cmd.Parameters.AddWithValue("@ProductName", productName);
                    cmd.Parameters.AddWithValue("@ExcludeId", excludeId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // =========================================
        // 4️⃣ CRUD: Invoice + InvoiceDetail
        // =========================================
        public int InsertInvoice(Invoice invoice, List<InvoiceDetail> details)
        {
             var conn = CreateConnection();
            conn.Open();
             var tran = conn.BeginTransaction();

            try
            {
                string sqlInvoice = @"INSERT INTO Invoice (InvoiceCode, InvoiceDate, Type, TotalAmount, Profit)
                                      VALUES (@InvoiceCode, @InvoiceDate, @Type, @TotalAmount, @Profit);
                                      SELECT last_insert_rowid();";
                int invoiceId = conn.ExecuteScalar<int>(sqlInvoice, invoice, tran);

                foreach (var d in details)
                {
                    d.InvoiceId = invoiceId;
                    string sqlDetail = @"INSERT INTO InvoiceDetail (InvoiceId, ProductId, Quantity, UnitPrice, Total)
                                         VALUES (@InvoiceId, @ProductId, @Quantity, @UnitPrice, @Total)";
                    conn.Execute(sqlDetail, d, tran);

                    // Cập nhật số lượng tồn kho
                    if (invoice.Type == "Export")
                    {
                        conn.Execute("UPDATE Product SET Quantity = Quantity - @q WHERE Id = @id", new { q = d.Quantity, id = d.ProductId }, tran);
                    }
                    else
                    {
                        conn.Execute("UPDATE Product SET Quantity = Quantity + @q WHERE Id = @id", new { q = d.Quantity, id = d.ProductId }, tran);
                    }
                }

                tran.Commit();
                return invoiceId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public IEnumerable<Invoice> GetInvoices()
        {
             var conn = CreateConnection();
            return conn.Query<Invoice>("SELECT * FROM Invoice ORDER BY InvoiceDate DESC");
        }

        public IEnumerable<InvoiceDetail> GetInvoiceDetails(int invoiceId)
        {
             var conn = CreateConnection();
            string sql = @"SELECT d.*, p.ProductName, p.Series 
                           FROM InvoiceDetail d 
                           LEFT JOIN Product p ON d.ProductId = p.Id
                           WHERE d.InvoiceId = @invoiceId";
            return conn.Query<InvoiceDetail>(sql, new { invoiceId });
        }

        // =========================================
        // 5️⃣ Báo cáo tồn kho
        // =========================================
        public IEnumerable<StockReport> GetStockReport()
        {
             var conn = CreateConnection();
            string sql = @"SELECT p.ProductName, p.Series, pt.TypeName AS ProductType,
                                  p.Quantity, p.CostPrice, p.SellPrice,
                                  (p.SellPrice - p.CostPrice) * p.Quantity AS Profit,
                                  p.ImportDate AS LastUpdated
                           FROM Product p
                           LEFT JOIN ProductType pt ON p.ProductTypeId = pt.Id";
            return conn.Query<StockReport>(sql);
        }

        // =========================================
        // 6️⃣ Tổng quan Dashboard
        // =========================================
        public DashboardSummary GetDashboardSummary()
        {
             var conn = CreateConnection();
            string sql = @"
               SELECT 
    (SELECT COUNT(*) FROM Product) AS TotalProducts,
    (SELECT IFNULL(SUM(Quantity), 0) FROM Product) AS TotalInStock,
    (SELECT IFNULL(SUM(CostPrice * Quantity), 0) FROM Product) AS TotalImportValue,
    (SELECT IFNULL(SUM(d.Total), 0)
     FROM Invoice i
     JOIN InvoiceDetail d ON i.Id = d.InvoiceId
     WHERE i.Type = 'Export') AS TotalExportValue,
    (SELECT IFNULL(SUM((d.UnitPrice - p.CostPrice) * d.Quantity), 0)
     FROM Invoice i
     JOIN InvoiceDetail d ON i.Id = d.InvoiceId
     JOIN Product p ON d.ProductId = p.Id
     WHERE i.Type = 'Export') AS TotalProfit
";
            return conn.QueryFirstOrDefault<DashboardSummary>(sql);
        }


        public IEnumerable<RevenueStatistic> GetRevenueByMonth(int year)
        {
            var conn = CreateConnection();
            string sql = @"
        SELECT 
            strftime('%m', InvoiceDate) AS Month,
            SUM(TotalAmount) AS TotalRevenue,
            SUM(Profit) AS TotalProfit
        FROM Invoice
        WHERE strftime('%Y', InvoiceDate) = @year
        GROUP BY strftime('%m', InvoiceDate)
        ORDER BY Month";
            return conn.Query<RevenueStatistic>(sql, new { year = year.ToString() });

        }

    }
}
