using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using Oracle.ManagedDataAccess.Client;

namespace DigitalPlatform.rms
{
    public static class CommandExtension
    {
        public static DbParameter NewParameter(
this DbCommand command,
string name,
DbType type = DbType.String,
int size = -1,
object value = null)
        {
            if (command is OracleCommand)
            {
                var oracle_command = (OracleCommand)command;
                oracle_command.BindByName = true;
                var param1 = new OracleParameter();
                param1.ParameterName = name.Replace("@", ":");
                if (type == DbType.String)
                    param1.OracleDbType = OracleDbType.NVarchar2;
                else
                    param1.DbType = type;
                param1.Value = value;
                if (size != -1)
                    param1.Size = size;
                oracle_command.Parameters.Add(param1);
                return param1;
            }

            {
                var param1 = command.CreateParameter();
                param1.ParameterName = name;
                param1.DbType = type;
                param1.Value = value;
                if (size != -1)
                    param1.Size = size;
                command.Parameters.Add(param1);

                return param1;
            }
        }

        // https://stackoverflow.com/questions/38005393/how-to-get-dapper-dynamic-as-case-insensitive
        public static IDictionary<string, T> ToCaseInsensitiveDictionary<T>(this IDictionary<string, T> source)
        {
            var target = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in source)
            {
                target[entry.Key] = entry.Value;
            }

            return target;
        }
    }
}
