using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public static class UsersFromSQL
    {
        public static List<UserSQL> Do()
        {
            var result = new List<UserSQL>();
            string connectionSql = @"Server=AKUSHNIR\MSSQLSERVER2014; Database=Kadr; uid=test; pwd=123456;";
            string query = "SELECT [coden],[namep] ,[iname] ,[fname] FROM [Kadr].[dbo].[Kadr] WHERE datezv IS NULL";
            using (var connection = new SqlConnection(connectionSql))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        var user = new UserSQL { Id = Convert.ToInt32(reader["coden"]), FirstName = reader["iname"].ToString(), LastName = reader["namep"].ToString(), Surname = reader["fname"].ToString(), Vaga = 0 };
                        result.Add(user);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    reader.Close();
                }
            }
            return result;
        }
    }

    public class UserSQL
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Surname { get; set; }
        public long TelegramId { get; set; }
        public float Vaga { get; set; }
    }
}
