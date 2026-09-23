namespace TableDBMS.DataTypes
{
    public interface IDataType
    {
        string Name { get; }

        bool IsValid(string? value);

        object? Parse(string? value);
    }
}