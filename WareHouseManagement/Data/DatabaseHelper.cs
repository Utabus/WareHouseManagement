using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
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
                    (SELECT IFNULL(SUM(SellPrice * Quantity), 0) FROM Product WHERE IsSold=1) AS TotalExportValue,
                    (SELECT IFNULL(SUM((SellPrice - CostPrice) * Quantity), 0) FROM Product WHERE IsSold=1) AS TotalProfit";
            return conn.QueryFirstOrDefault<DashboardSummary>(sql);
        }
    }
}
