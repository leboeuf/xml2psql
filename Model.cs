using System.Collections.Generic;

namespace xml2psql.Model
{
    public class Table
    {
        public string Name { get; set; }
        public string UniqueIndex { get; set; }
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
