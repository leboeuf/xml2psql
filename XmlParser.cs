using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using xml2psql.Model;

namespace xml2psql
{
    public static class XmlParser
    {
        public static IEnumerable<Table> ParseXml(XDocument xml)
        {
            var tableNodes = xml.XPathSelectElements("/xml2psql/Schema/Tables/Table");
            var tables = tableNodes.Select(GetTable);
            
            return tables;
        }

        private static Column GetColumn(XElement xml)
        {
            var name = xml.Attribute("name")?.Value;
            var dataType = xml.Attribute("type")?.Value.ToLowerInvariant();
            var hasIndex = xml.Attribute("hasindex")?.Value.ToLowerInvariant() == "true";
            var isPrimaryKey = xml.Attribute("isprimarykey")?.Value.ToLowerInvariant() == "true";
            var isUnique = xml.Attribute("unique")?.Value.ToLowerInvariant() == "true";
            var notNull = xml.Attribute("notnull")?.Value.ToLowerInvariant() == "true";

            return new Column
            {
                Name = name,
                DataType = dataType,
                IsPrimaryKey = isPrimaryKey,
                HasIndex = hasIndex,
                Unique = isUnique,
                NotNull = notNull
            };
        }

        private static Table GetTable(XElement xml)
        {
            var name = xml.Attribute("name")?.Value;
            var columnNodes = xml.XPathSelectElements("Column");

            return new Table
            {
                Name = name,
                Columns = columnNodes.Select(GetColumn)
            };
        }
    }
}