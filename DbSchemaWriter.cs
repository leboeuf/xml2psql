using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xml2psql.Model;

namespace xml2psql
{
    public static class DbSchemaWriter
    {
        public static async Task Write(string connectionString, IEnumerable<Table> tables)
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();

            // Check whether we need to enable uuid support
            var hasUuidPrimaryKey = tables.Any(t => t.Columns.Any(c => c.DataType.ToLowerInvariant() == "uuid" && c.IsPrimaryKey));
            if (hasUuidPrimaryKey)
            {
                Console.WriteLine("Warning: Found PK columns with UUID data type, please remember to run 'CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";' on your database (this script won't run it because the user needs SUPERADMIN privileges).");
            }

            // Create tables
            foreach (var table in tables)
            {
                command.CommandText = $"CREATE TABLE {table.Name} ({Environment.NewLine}{GetColumnDefinitions(table.Columns)})";
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

        public static async Task ExecuteSql(string connectionString, string sql)
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();

            try
            {
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while committing database transaction: {e.Message}");
                await transaction.RollbackAsync();
            }
        }

        private static string GetColumnDefinitions(IEnumerable<Column> columns)
        {
            var hasCompositePrimaryKey = columns.Count(c => c.IsPrimaryKey) > 1;
                    
            var definitions = columns.Select(c => GetColumnDefinition(c, hasCompositePrimaryKey)).ToList();

            if (hasCompositePrimaryKey)
            {
                var keyColumns = columns.Where(c => c.IsPrimaryKey).Select(c => c.Name);
                definitions.Add($"PRIMARY KEY ({string.Join(", ", keyColumns)})");
            }

            return string.Join($",{Environment.NewLine}", definitions);
        }

        private static string GetColumnDefinition(Column column, bool tableHasCompositePrimaryKey)
        {
            var sb = new StringBuilder($"{column.Name} {column.DataType}");

            if (column.IsPrimaryKey)
            {
                if (!tableHasCompositePrimaryKey)
                {
                    sb.Append(" PRIMARY KEY");
                }

                if (column.DataType.ToLowerInvariant() == "uuid")
                {
                    sb.Append(" DEFAULT uuid_generate_v4()");
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

            if (!string.IsNullOrWhiteSpace(column.ForeignKey))
            {
                sb.Append($" REFERENCES {column.ForeignKey}");
            }

            return sb.ToString();
        }
    }
}
