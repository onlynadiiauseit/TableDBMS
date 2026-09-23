namespace TableDBMS.DataTypes
{
    public class IntegerType : IDataType
    {
        public string Name => "Integer";

        public bool IsValid(string? value)
        {
            return int.TryParse(value, out _);
        }

        public object? Parse(string? value)
        {
            if (int.TryParse(value, out int result))
                return result;

            return null;
        }
    }
}