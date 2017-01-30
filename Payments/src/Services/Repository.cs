using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using Avalarin.Data;
using Payments.Model;
using static System.String;

namespace Payments.Services {
    public class Repository {
        private readonly string _connectionString;

        private readonly ConcurrentDictionary<int, Wallet> _walletsCache = new ConcurrentDictionary<int, Wallet>();
        private readonly ConcurrentDictionary<int, Category> _categoriesCache = new ConcurrentDictionary<int, Category>();
        private readonly ConcurrentDictionary<int, Payment> _paymentsCache = new ConcurrentDictionary<int, Payment>();

        public Repository(string connectionString) {
            _connectionString = connectionString;
        }

        private SqlCeConnection GetConnection() {
            return new SqlCeConnection(_connectionString);
        }

        private T WithConnection<T>(Func<SqlCeConnection, T> action) {
            using (var conn = GetConnection()) {
                return action(conn);
            }
        }

        public Wallet CreateWallet(string name) {
            using (var conn = GetConnection()) {
                conn.Text("insert into wallets (name, balance) values (@name, 0)")
                    .WithParameter("name", name)
                    .ExecuteNonQuery();

                var id = conn.Text("select @@identity as id").ExecuteAndReadFirstOrDefault(r => (int)r.Value<decimal>("id"));

                return new Wallet() {
                    Id = id,
                    Name = name
                };
            }
        }

        public Wallet GetWallet(int id) {
            return _walletsCache.GetOrAdd(id, i => WithConnection(conn => 
                conn.Text("select * from wallets where id = @id")
                    .WithParameter("id", id)
                    .ExecuteAndReadFirstOrDefault(MapWallet)));
        }

        public Wallet[] GetWallets() {
            using (var conn = GetConnection()) {
                return conn.Text("select * from wallets")
                            .ExecuteAndReadAll(MapWallet)
                            .ToArray();
            }
        }

        public Category CreateCategory(string name) {
            using (var conn = GetConnection()) {
                conn.Text("insert into categories (name) values (@name)")
                    .WithParameter("name", name)
                    .ExecuteNonQuery();

                var id = conn.Text("select @@identity as id").ExecuteAndReadFirstOrDefault(r => (int)r.Value<decimal>("id"));

                return new Category() {
                    Id = id,
                    Name = name
                };
            }
        }

        public Category GetCategory(int id) {
            return _categoriesCache.GetOrAdd(id, i => WithConnection(conn =>
                conn.Text("select * from categories where id = @id")
                    .WithParameter("id", id)
                    .ExecuteAndReadFirstOrDefault(MapCategory)));
        }

        public Category[] GetCategories() {
            using (var conn = GetConnection()) {
                return conn.Text("select * from categories")
                            .ExecuteAndReadAll(MapCategory)
                            .ToArray();
            }
        }

        public Payment CreatePayment(int walletId, PaymentType type, string name, DateTime date, decimal amount, int[] categories) {
            using (var conn = GetConnection()) {
                conn.Text("insert into payments (wallet_id, type, name, date, amount) " +
                          "values (@walletId, @type, @name, @date, @amount)")
                    .WithParameters(new { walletId, name, date, amount, type = (int)type })
                    .ExecuteNonQuery();

                var id = conn.Text("select @@identity as id").ExecuteAndReadFirstOrDefault(r => (int)r.Value<decimal>("id"));

                foreach (var categoryId in categories) {
                    conn.Text("insert into payment_categories (payment_id, category_id) " +
                          "values (@paymentId, @categoryId)")
                    .WithParameters(new { paymentId = id, categoryId })
                    .ExecuteNonQuery();
                }

                var balance = conn.Text("select type, amount from payments where wallet_id = @wallet_id")
                                      .WithParameter("wallet_id", walletId)
                                      .ExecuteAndReadAll(r => {
                                            var a = r.Value<decimal>("amount");
                                            var t = (PaymentType)r.Value<int>("type");
                                            if (t == PaymentType.Outcome) a *= -1;
                                            return a;
                                        })
                                        .Sum();

                var wallet = GetWallet(walletId);
                wallet.Balance = balance;

                conn.Text("update wallets set balance = @balance where id = @walletId")
                        .WithParameters(new { balance, walletId })
                        .ExecuteNonQuery();

                return new Payment() {
                    Id = id,
                    Type = type,
                    Date = date,
                    Name = name,
                    WalletId = walletId,
                    Wallet = wallet,
                    Amount = amount,
                    Categories = categories.Select(GetCategory).ToArray()
                };
            }
        }

        public Payment GetPayment(int id) {
            return _paymentsCache.GetOrAdd(id, i => WithConnection(conn => {
                var p = conn.Text("select * from payments where id = @id")
                    .WithParameter("id", id)
                    .ExecuteAndReadFirstOrDefault(MapPayment);

                p.Categories = conn.Text("select c.* from payment_categories ps " +
                                    "  inner join categories c on ps.category_id = c.category_id " +
                                    "  where pc.payment_id = @id")
                                .WithParameter("id", id)
                                .ExecuteAndReadAll(MapCategory)
                                .ToArray();

                return p;
            }));
        }

        public Payment[] GetPayments(int? walletId, int? categoryId) {
            var query = "select p.* from payments as p";
            var parameters = new Dictionary<string, object>();

            if (categoryId.HasValue) {
                query += " inner join payment_categories as pc on pc.payment_id = p.id";
            }

            if (walletId.HasValue) {
                query += " where p.wallet_id = @wallet_id";
                parameters.Add("wallet_id", walletId.Value);
            }

            using (var conn = GetConnection()) {
                var payments = conn.Text(query)
                                    .WithParameters(parameters)
                                    .ExecuteAndReadAll(MapPayment)
                                    .ToArray();

                if (!payments.Any()) return payments;

                var idsStr = Join(", ", payments.Select(p => p.Id));

                var categories = conn.Text("select pc.payment_id, c.* from categories c " +
                                    "  inner join payment_categories pc on pc.category_id = c.id " +
                                    $"  where pc.payment_id in ({idsStr})")
                                .ExecuteAndReadAll(r => new { paymentId = r.Value<int>("payment_id"), category = MapCategory(r) })
                                .GroupBy(c => c.paymentId, c => c.category)
                                .ToDictionary(c => c.Key, c => c.ToArray());

                foreach (var payment in payments) {
                    Category[] paymentCategories;
                    if (categories.TryGetValue(payment.Id, out paymentCategories)) {
                        payment.Categories = paymentCategories;
                    }
                }

                return payments;
            }
        }

        private Wallet MapWallet(IDataReader reader) {
            return new Wallet() {
                Id = reader.Value<int>("id"),
                Name = reader.Value<string>("name"),
                Balance = reader.Value<decimal>("balance")
            };
        }

        private Category MapCategory(IDataReader reader) {
            return new Category() {
                Id = reader.Value<int>("id"),
                Name = reader.Value<string>("name")
            };
        }

        private Payment MapPayment(IDataReader reader) {
            var payment = new Payment() {
                Id = reader.Value<int>("id"),
                Name = reader.Value<string>("name"),
                Date = reader.Value<DateTime>("date"),
                Type = (PaymentType)reader.Value<int>("type"),
                WalletId = reader.Value<int>("wallet_id"),
                Wallet = GetWallet(reader.Value<int>("wallet_id"))
            };
            var amount = reader.Value<decimal>("amount");
            if (payment.Type == PaymentType.Outcome) {
                amount *= -1;
            }
            payment.Amount = amount;
            return payment;
        }
    }
}