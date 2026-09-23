using System.Globalization;

namespace TableDBMS.DataTypes
{
    public class ColorType : IDataType
    {
        public string Name => "Color";

        public bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();

            if (value.Length != 7 ||
                value[0] != '#')
            {
                return false;
            }

            return int.TryParse(
                value[1..],
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out _
            );
        }

        public object? Parse(string? value)
        {
            if (!IsValid(value))
                return null;

            return value!.Trim().ToUpperInvariant();
        }
    }
}