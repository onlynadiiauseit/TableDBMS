namespace TableDBMS.DataTypes
{
    public class StringType : IDataType
    {
        public string Name => "String";

        public bool IsValid(string? value)
        {
            return value != null;
        }

        public object? Parse(string? value)
        {
            return value;
        }
    }
}