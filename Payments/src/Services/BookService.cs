using System;
using System.Data.SqlServerCe;
using System.IO;
using Avalarin.Data;

namespace Payments.Services {
    public class BookService {

        public Repository OpenBook(string fileName) {
            if (!File.Exists(fileName)) {
                throw new FileNotFoundException("File not found", fileName);
            }

            var connectionString = CreateConnectionString(fileName);
            using (var conn = new SqlCeConnection(connectionString)) {
                var version = conn.Text("select version from info")
                    .ExecuteAndReadFirstOrDefault(r => r.Value<string>("version"));
                if (version != "1.0") {
                    throw new InvalidOperationException("Invalid version " + version);
                }
            }

            return new Repository(connectionString);
        }

        public Repository CreateBook(string fileName) {
            if (File.Exists(fileName)) {
                File.Delete(fileName);
            }

            var connectionString = CreateConnectionString(fileName);
            var en = new SqlCeEngine(connectionString);
            en.CreateDatabase();

            using (var conn = new SqlCeConnection(connectionString)) {
                conn.Text(@"create table info (
                                version nvarchar(10) not null
                            )").ExecuteNonQuery();
                conn.Text("insert into info (version) values ('1.0')").ExecuteNonQuery();

                conn.Text(@"create table wallets (
                                id int not null identity(1,1) primary key,
                                name nvarchar(200) not null,
                                balance decimal(10,2) default 0
                            )").ExecuteNonQuery();
                conn.Text(@"create table categories (
                                id int not null identity(1,1) primary key,
                                name nvarchar(200) not null
                            )").ExecuteNonQuery();

                conn.Text(@"create table payment_categories (
                                category_id int not null,
                                payment_id int not null
                            )").ExecuteNonQuery();

                conn.Text(@"create table payments (
                                id int not null identity(1,1) primary key,
                                name nvarchar(200) not null,
                                date datetime not null,
                                wallet_id int not null,
                                type int not null,
                                amount decimal(10,2) not null
                            )").ExecuteNonQuery();
            }

            return OpenBook(fileName);
        }

        private string CreateConnectionString(string fileName) {
            return $"DataSource=\"{fileName}\"; Password=\"mypassword\"";
        }

    }
}