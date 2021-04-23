using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Npgsql;

namespace xml2psql.Model
{
    public class Table
    {
        public string Name { get; set; }
        public IEnumerable<Column> Columns { get; set; }
    }

    public class Column
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool HasIndex { get; set; }
        public bool Unique { get; set; }
        public bool NotNull { get; set; }
        public string ForeignKey { get; set; }
    }
}
