using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Npgsql;
using xml2psql.Model;

namespace xml2psql
{
    public static class DbSchemaWriter
    {
        public static async Task Write(string connectionString, IEnumerable<Table> tables)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                using (var command = connection.CreateCommand())
                {
                    // Check whether we need to enable uuid support
                    var hasUuidPrimaryKey = tables.Any(t => t.Columns.Any(c => c.DataType.ToLowerInvariant() == "uuid" && c.IsPrimaryKey));
                    if (hasUuidPrimaryKey)
                    {
                        Console.WriteLine("Warning: Found PK columns with UUID data type, please remember to run 'CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";' on your database (this script won't run it because the user needs SUPERADMIN privileges).");
                    }

                    // Create tables
                    foreach (var table in tables)
                    {
                        command.CommandText = $"CREATE TABLE {table.Name} ({GetColumnDefinitions(table.Columns)})";
                        await command.ExecuteNonQueryAsync();

                        var columnsWithIndexes = table.Columns.Where(c => c.HasIndex).ToArray();
                        if (columnsWithIndexes.Count() == 0)
                        {
                            continue;
                        }

                        foreach (var column in columnsWithIndexes)
                        {
                            command.CommandText = $"CREATE INDEX on {table.Name} ({column.Name})";
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    try
                    {
                        await transaction.CommitAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error while committing database transaction: {e.Message}");
                        await transaction.RollbackAsync();
                    }
                }
            }
        }

        private static string GetColumnDefinitions(IEnumerable<Column> columns)
        {
            var definitions = columns.Select(GetColumnDefinition);
            return string.Join($",{Environment.NewLine}", definitions);
        }

        private static string GetColumnDefinition(Column column)
        {
            var sb = new StringBuilder($"{column.Name} {column.DataType}");

            if (column.IsPrimaryKey)
            {
                sb.Append(" PRIMARY KEY");

                if (column.DataType.ToLowerInvariant() == "uuid")
                {
                    sb.Append(" DEFAULT uuid_generate_v1mc()");
                }
            }

            if (column.Unique)
            {
                sb.Append(" UNIQUE");
            }

            if (column.NotNull)
            {
                sb.Append(" NOT NULL");
            }

            return sb.ToString();
        }
    }
}
