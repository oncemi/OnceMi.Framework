using FreeSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Config
{
    public class DbConnectionStringsNode
    {
        private string _connectionString = null;

        public string Name { get; set; }

        public DataType DbType { get; set; }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("Connection string can not null");
                }
                
                if (DbType == DataType.Sqlite)
                {
                    string connStr = value;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        connStr = connStr.Replace("/", "\\");
                    else
                        connStr = connStr.Replace("\\", "/");
                    if (connStr.Contains("{root}", StringComparison.OrdinalIgnoreCase))
                        _connectionString = connStr.Replace("{root}", AppContext.BaseDirectory);
                    else
                        _connectionString = connStr;
                }
                else
                {
                    _connectionString = value;
                }
            }
        }
    }
}
