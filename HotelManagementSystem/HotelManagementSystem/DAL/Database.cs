
using MySql.Data.MySqlClient;

namespace HotelManagementSystem.DAL
{
    public static class Database
    {
        private static readonly string connectionString =
            "Server=localhost;Port=3306;Database=HotelDB;Uid=root;Pwd=123456;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
