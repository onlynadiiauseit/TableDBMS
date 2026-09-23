namespace TableDBMS.DataTypes
{
    public class CharType : IDataType
    {
        public string Name => "Char";

        public bool IsValid(string? value)
        {
            return value is { Length: 1 };
        }

        public object? Parse(string? value)
        {
            if (!IsValid(value))
                return null;

            return value![0];
        }
    }
}