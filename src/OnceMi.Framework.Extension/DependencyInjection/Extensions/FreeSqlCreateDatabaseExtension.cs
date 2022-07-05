using FreeSql;
using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    internal static class FreeSqlCreateDatabaseExtension
    {
        /// <summary>
        /// 请在UseConnectionString配置后调用此方法
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static FreeSqlBuilder CreateDatabaseIfNotExists(this FreeSqlBuilder @this)
        {
            FieldInfo dataTypeFieldInfo = @this.GetType().GetField("_dataType", BindingFlags.NonPublic | BindingFlags.Instance);

            if (dataTypeFieldInfo is null)
            {
                throw new ArgumentException("_dataType is null");
            }

            string connectionString = GetConnectionString(@this);
            DataType dbType = (DataType)dataTypeFieldInfo.GetValue(@this);

            switch (dbType)
            {
                case DataType.MySql:
                    return @this.CreateDatabaseIfNotExistsMySql(connectionString);
                case DataType.SqlServer:
                    return @this.CreateDatabaseIfNotExistsSqlServer(connectionString);
                case DataType.PostgreSQL:
                    return @this.CreateDatabaseIfNotExistsPgSql(connectionString);
                case DataType.Oracle:
                    break;
                case DataType.Sqlite:
                    return @this.CreateDatabaseIfNotExistsSqlite(connectionString);
                case DataType.OdbcOracle:
                    break;
                case DataType.OdbcSqlServer:
                    return @this.CreateDatabaseIfNotExists_ODBCSqlServer(connectionString);
                case DataType.OdbcMySql:
                    return @this.CreateDatabaseIfNotExists_ODBCMySql(connectionString);
                case DataType.OdbcPostgreSQL:
                    break;
                case DataType.Odbc:
                    break;
                case DataType.OdbcDameng:
                    break;
                case DataType.MsAccess:
                    break;
                case DataType.Dameng:
                    break;
                case DataType.OdbcKingbaseES:
                    break;
                case DataType.ShenTong:
                    break;
                case DataType.KingbaseES:
                    break;
                case DataType.Firebird:
                    break;
                default:
                    break;
            }

            throw new Exception($"不支持创建数据库");
        }

        public static FreeSqlBuilder CreateDatabaseIfNotExistsSqlite(this FreeSqlBuilder @this,
            string connectionString = "")
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder(connectionString);
            FileInfo file = new FileInfo(builder.DataSource);
            if (file.Exists)
            {
                return @this;
            }
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            return @this;
        }

        public static FreeSqlBuilder CreateDatabaseIfNotExistsMySql(this FreeSqlBuilder @this,
        string connectionString = "")
        {
            if (connectionString == "")
            {
                connectionString = GetConnectionString(@this);
            }
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(connectionString);
            string createDatabaseSql = $"USE mysql;CREATE DATABASE IF NOT EXISTS `{builder.Database}` CHARACTER SET '{builder.CharacterSet}' COLLATE 'utf8mb4_general_ci'";
            string queryDbSql = "SHOW DATABASES";
            using MySqlConnection cnn = new MySqlConnection(
                $"Data Source={builder.Server};Port={builder.Port};User ID={builder.UserID};Password={builder.Password};Initial Catalog=;Charset=utf8;SslMode=none;Max pool size=1");
            cnn.Open();
            //判断数据库是否存在
            using (MySqlCommand cmd = cnn.CreateCommand())
            {
                cmd.CommandText = queryDbSql;
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                {
                    using DataSet dataset = new DataSet();
                    adapter.Fill(dataset);

                    using DataTable dt = dataset.Tables[0];
                    DataRowCollection rows = dt.Rows;
                    foreach (DataRow dr in rows)
                    {
                        string dbName = dr[0].ToString();
                        if (dbName.Equals(builder.Database, StringComparison.OrdinalIgnoreCase))
                            return @this;
                    }
                }
            }
            using (MySqlCommand cmd = cnn.CreateCommand())
            {
                cmd.CommandText = createDatabaseSql;
                cmd.ExecuteNonQuery();
            }

            return @this;
        }

        public static FreeSqlBuilder CreateDatabaseIfNotExistsSqlServer(this FreeSqlBuilder @this, string connectionString = "")
        {
            if (connectionString == "")
            {
                connectionString = GetConnectionString(@this);
            }
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            string createDatabaseSql;
            if (!string.IsNullOrEmpty(builder.AttachDBFilename))
            {
                string fileName = ExpandFileName(builder.AttachDBFilename);
                string name = Path.GetFileNameWithoutExtension(fileName);
                string logFileName = Path.ChangeExtension(fileName, ".ldf");
                createDatabaseSql = @$"CREATE DATABASE {builder.InitialCatalog}   on  primary   
                (
                    name = '{name}',
                    filename = '{fileName}'
                )
                log on
                (
                    name= '{name}_log',
                    filename = '{logFileName}'
                )";
            }
            else
            {
                createDatabaseSql = @$"CREATE DATABASE {builder.InitialCatalog}";
            }

            using SqlConnection cnn =
                new SqlConnection(
                    $"Data Source={builder.DataSource};Integrated Security = True;User ID={builder.UserID};Password={builder.Password};Initial Catalog=master;Min pool size=1;Encrypt={builder.Encrypt};TrustServerCertificate={builder.TrustServerCertificate};Trusted_Connection=False;");
            cnn.Open();
            using SqlCommand cmd = cnn.CreateCommand();
            cmd.CommandText = $"select * from sysdatabases where name = '{builder.InitialCatalog}'";

            SqlDataAdapter apter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            apter.Fill(ds);

            if (ds.Tables[0].Rows.Count == 0)
            {
                cmd.CommandText = createDatabaseSql;
                cmd.ExecuteNonQuery();
            }

            return @this;
        }

        private static string ExpandFileName(string fileName)
        {
            if (fileName.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
            {
                var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (string.IsNullOrEmpty(dataDirectory))
                {
                    dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }
                string name = fileName.Replace("\\", "").Replace("/", "").Substring("|DataDirectory|".Length);
                fileName = Path.Combine(dataDirectory, name);
            }
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            return Path.GetFullPath(fileName);
        }


        private static string GetConnectionString(FreeSqlBuilder @this)
        {
            Type type = @this.GetType();
            FieldInfo fieldInfo =
                type.GetField("_masterConnectionString", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo is null)
            {
                throw new ArgumentException("_masterConnectionString is null");
            }
            return fieldInfo.GetValue(@this).ToString();
        }


        public static FreeSqlBuilder CreateDatabaseIfNotExistsPgSql(this FreeSqlBuilder @this,
      string connectionString = "")
        {
            if (connectionString == "")
            {
                connectionString = GetConnectionString(@this);
            }
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connectionString);
            string createDatabaseSql = $"DROP DATABASE IF EXISTS \"{builder.Database}\"; CREATE DATABASE \"{builder.Database}\" WITH OWNER = \"{builder.Username}\"";
            string queryDbSql = "SELECT datname FROM pg_database";
            using NpgsqlConnection cnn = new NpgsqlConnection(
                $"Host={builder.Host};Port={builder.Port};Database=postgres;Username={builder.Username};Password={builder.Password};Pooling=true");
            cnn.Open();
            //判断数据库是否存在
            using (NpgsqlCommand cmd = cnn.CreateCommand())
            {
                cmd.CommandText = queryDbSql;
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                {
                    using DataSet dataset = new DataSet();
                    adapter.Fill(dataset);

                    using DataTable dt = dataset.Tables[0];
                    DataRowCollection rows = dt.Rows;
                    foreach (DataRow dr in rows)
                    {
                        string dbName = dr[0].ToString();
                        if (dbName.Equals(builder.Database, StringComparison.OrdinalIgnoreCase))
                            return @this;
                    }
                }
            }
            //创建数据库
            using (NpgsqlCommand cmd = cnn.CreateCommand())
            {
                cmd.CommandText = createDatabaseSql;
                cmd.ExecuteReader();
            }
            return @this;
        }

        #region ODBC
        public static FreeSqlBuilder CreateDatabaseIfNotExists_ODBCMySql(this FreeSqlBuilder @this,
          string connectionString = "")
        {
            if (connectionString == "")
            {
                connectionString = GetConnectionString(@this);
            }

            OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder(connectionString);

            string createDatabaseSql =
                $"USE mysql;CREATE DATABASE IF NOT EXISTS `{builder["DATABASE"]}` CHARACTER SET '{builder["Charset"]}' COLLATE 'utf8mb4_general_ci'";

            using OdbcConnection cnn = new OdbcConnection(
                $"Data Source={builder["Server"]};Port={builder["Port"]};User ID={builder["UID"]};Password={builder["PWD"]};Initial Catalog=mysql;Charset=utf8;SslMode=none;Max pool size=1");
            cnn.Open();
            using (OdbcCommand cmd = cnn.CreateCommand())
            {
                cmd.CommandText = createDatabaseSql;
                cmd.ExecuteNonQuery();
            }

            return @this;
        }


        public static FreeSqlBuilder CreateDatabaseIfNotExists_ODBCSqlServer(this FreeSqlBuilder @this,
            string connectionString = "")
        {
            if (connectionString == "")
            {
                connectionString = GetConnectionString(@this);
            }

            OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder(connectionString);


            string createDatabaseSql = "";
            if (builder.ContainsKey("AttachDBFilename") && !string.IsNullOrEmpty(builder["AttachDBFilename"].ToString()))
            {
                string fileName = ExpandFileName(builder["AttachDBFilename"].ToString());
                string name = Path.GetFileNameWithoutExtension(fileName);
                string logFileName = Path.ChangeExtension(fileName, ".ldf");
                createDatabaseSql = @$"CREATE DATABASE {builder["Initial Catalog"]}   on  primary   
                (
                    name = '{name}',
                    filename = '{fileName}'
                )
                log on
                (
                    name= '{name}_log',
                    filename = '{logFileName}'
                )";
            }
            else
            {
                createDatabaseSql = @$"CREATE DATABASE {builder["Initial Catalog"]}";
            }

            //一个空格都不能多
            //string MasterConnectionString = "Driver={SQL Server};Server=.;Initial Catalog=master;Uid=sa;Pwd=123456";
            string MasterConnectionString = $"Driver={{SQL Server}};Server={builder["Server"].ToString()};Initial Catalog=master;Uid={builder["Uid"].ToString()};Pwd={builder["Pwd"].ToString()};";
            using OdbcConnection cnn = new OdbcConnection(MasterConnectionString);

            cnn.Open();
            using OdbcCommand cmd = cnn.CreateCommand();
            cmd.CommandText = $"select * from sysdatabases where name = '{builder["Initial Catalog"]}'";

            OdbcDataAdapter apter = new OdbcDataAdapter(cmd);
            DataSet ds = new DataSet();
            apter.Fill(ds);

            if (ds.Tables[0].Rows.Count == 0)
            {
                cmd.CommandText = createDatabaseSql;
                cmd.ExecuteNonQuery();
            }

            return @this;
        }
        #endregion
    }
}