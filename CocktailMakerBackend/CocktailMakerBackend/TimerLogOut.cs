using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace CocktailMakerBackend
{
    public static class TimerLogOut
    {
        [FunctionName("TimerLogOut")]
        public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 */30 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger 'TimerLogOut' executed at: {DateTime.Now}");
            string connectionstring = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionstring;
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "select * from tbl_user where session_id is not null;";
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                DateTime t = GetDateTime(reader["session_time"].ToString());
                                if(t.AddMinutes(30) < DateTime.Now)
                                {
                                    LogOut(reader["ID"].ToString(), log);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> TimerLogOut/ReadAllToUpdate");
            }
        }
        public static DateTime GetDateTime(String value)
        {
            double unixTimestamp = Convert.ToDouble(value);
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimestamp).ToLocalTime();
            return dtDateTime;
        }

        public static async void LogOut(string id, ILogger log)
        {
            string connectionstring = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionstring;
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "update tbl_user set session_id = NULL, session_time = NULL where ID = @ID";
                        cmd.Parameters.AddWithValue("@ID", id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch(Exception ex)
            {
                log.LogError(ex + "        --------> TimerLogOut/LogOut");
            }
        }
    }
}
