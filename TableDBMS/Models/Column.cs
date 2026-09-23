using System.Text.Json.Serialization;
using TableDBMS.DataTypes;

namespace TableDBMS.Models
{
    public class Column
    {
        public string Name { get; set; } = string.Empty;

        public string DataTypeName { get; set; } = "String";

        [JsonIgnore]
        public IDataType DataType =>
            DataTypeFactory.Create(DataTypeName);

        public Column()
        {
        }

        public Column(string name, string dataTypeName)
        {
            Name = name;
            DataTypeName = dataTypeName;
        }

        public bool IsValid(string? value)
        {
            return DataType.IsValid(value);
        }
    }
}