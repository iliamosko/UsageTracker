using Npgsql;
using TimeTracker.Entities;

namespace UsageTracker.BackEnd
{
    internal static class SqlDatabase
    {
        const string accountsTable = "accounts";
        static NpgsqlDataSource dataSource;


        public static void CreateDatasource(string connectionString)
        {
            if (dataSource == null)
            {
                dataSource = NpgsqlDataSource.Create(connectionString);
            }
            else
            {
                throw new Exception("DataSource is already set");
            }
        }

        public static IEnumerable<User> ExecuteCommand(string command)
        {
            if (dataSource is null)
            {
                throw new NullReferenceException("Datasource is null");
            }

            if (command.Contains(accountsTable))
            {
                return QueryAccountsTable(command);
            }
            return Array.Empty<User>();
        }


        private static List<User> QueryAccountsTable(string command)
        {
            List<User> list = new List<User>();
            var query = dataSource.CreateCommand(command);

            using var reader = query.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new User((int)reader.GetValue(0), (string)reader.GetValue(1), (string)reader.GetValue(2), (string)reader.GetValue(3), (string)reader.GetValue(4)));
            }

            return list;
        }
    }
}
