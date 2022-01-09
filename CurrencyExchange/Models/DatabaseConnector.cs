using Microsoft.Data.Sqlite;
using System.Globalization;

namespace CurrencyExchange.Models
{
    public interface IDatabaseConnector
    {
        void createTables();
        void insertIntoKeys(string key);
        bool checkKey(string key);
        (List<ExchangeRate>, List<DateTime>) checkExchangeRates(string originCurrency, string targetCurrency, DateTime startDate, DateTime endDate);
        void insertIntoExchangeRates(List<ExchangeRate> rates);
        void insertIntoLogs(string currencies, DateTime startDate, DateTime endDate, string key);

    }
    public class DatabaseConnector : IDatabaseConnector
    {
        private SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder();

        public DatabaseConnector()
        {
            connectionStringBuilder.DataSource = "./CurrrencyExchangeDb.db";
        }

        public void createTables()
        {
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE name='Keys'";
                var name = command.ExecuteScalar();
                if (name == null)
                {
                    command.CommandText = "CREATE TABLE Keys(key VARCHAR(8));";
                    command.ExecuteNonQuery();
                }

                command.CommandText = "SELECT name FROM sqlite_master WHERE name='ExchangeRates'";
                name = command.ExecuteScalar();
                if (name == null)
                {
                    command.CommandText = "CREATE TABLE ExchangeRates(originCurrency VARCHAR(3),targetCurrency VARCHAR(3),date DATE, rate REAL, PRIMARY KEY(originCurrency,targetCurrency,date));";
                    command.ExecuteNonQuery();
                }

                command.CommandText = "SELECT name FROM sqlite_master WHERE name='Logs'";
                name = command.ExecuteScalar();
                if (name == null)
                {
                    command.CommandText = "CREATE TABLE Logs(id INTEGER PRIMARY KEY AUTOINCREMENT, currencies VARCHAR(100), startDate DATE, endDate DATE, key VARCHAR(8));";
                    command.ExecuteNonQuery();
                }
            }
        }

        public void insertIntoKeys(string key)
        {

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"INSERT INTO Keys VALUES('{key}')";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
        }

        public bool checkKey(string key)
        {

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT key FROM Keys WHERE key = '{key}'";
                var name = command.ExecuteScalar();
                if (name != null)
                    return true;
                else
                    return false;
            }

        }
        public (List<ExchangeRate>,List<DateTime>) checkExchangeRates(string originCurrency, string targetCurrency, DateTime startDate, DateTime endDate)
        {

            var selectResults = (new List<ExchangeRate>(), new List<DateTime>());

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT originCurrency, targetCurrency, date, rate FROM ExchangeRates WHERE originCurrency = '{originCurrency}'" +
                    $"AND targetCurrency = '{targetCurrency}' AND date BETWEEN '{startDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'";

                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        selectResults.Item1.Add(new ExchangeRate(reader.GetString(0), reader.GetString(1), reader.GetDateTime(2), reader.GetDouble(3)));
                        selectResults.Item2.Add(reader.GetDateTime(2));
                    }
                }
            }
            return selectResults;
        }

        public void insertIntoExchangeRates(List<ExchangeRate> rates)
        {

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    foreach (var rate in rates)
                    {
                        command.CommandText = $"INSERT OR IGNORE INTO ExchangeRates VALUES('{rate.originCurrency}', '{rate.targetCurrency}', '{rate.date.ToString("yyyy-MM-dd")}', {rate.rate.ToString(CultureInfo.InvariantCulture)})";
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public void insertIntoLogs(string currencies, DateTime startDate, DateTime endDate, string key)
        {
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"INSERT INTO Logs VALUES( NULL,'{currencies}','{startDate.ToString("yyyy-MM-dd")}','{endDate.ToString("yyyy-MM-dd")}', '{key}')";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
        }
    }
}
