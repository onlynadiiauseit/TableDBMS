using System.Globalization;

namespace TableDBMS.DataTypes
{
    public class ColorIntervalType : IDataType
    {
        public string Name => "ColorInvl";

        private readonly ColorType _colorType = new();

        public bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string[] parts = value.Split(
                "..",
                StringSplitOptions.None
            );

            if (parts.Length != 2)
                return false;

            string start = parts[0].Trim();
            string end = parts[1].Trim();

            if (!_colorType.IsValid(start) ||
                !_colorType.IsValid(end))
            {
                return false;
            }

            int startValue =
                ParseColorNumber(start);

            int endValue =
                ParseColorNumber(end);

            return startValue <= endValue;
        }

        public object? Parse(string? value)
        {
            if (!IsValid(value))
                return null;

            string[] parts = value!.Split(
                "..",
                StringSplitOptions.None
            );

            string start =
                parts[0]
                    .Trim()
                    .ToUpperInvariant();

            string end =
                parts[1]
                    .Trim()
                    .ToUpperInvariant();

            return $"{start}..{end}";
        }

        private static int ParseColorNumber(
            string color)
        {
            return int.Parse(
                color[1..],
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture
            );
        }
    }
}