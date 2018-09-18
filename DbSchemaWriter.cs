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
