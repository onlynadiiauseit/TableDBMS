using System.Globalization;

namespace TableDBMS.DataTypes
{
    public class RealType : IDataType
    {
        public string Name => "Real";

        public bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return double.TryParse(
                       value,
                       NumberStyles.Float,
                       CultureInfo.CurrentCulture,
                       out _)
                   ||
                   double.TryParse(
                       value,
                       NumberStyles.Float,
                       CultureInfo.InvariantCulture,
                       out _);
        }

        public object? Parse(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (double.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.CurrentCulture,
                    out double currentResult))
            {
                return currentResult;
            }

            if (double.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double invariantResult))
            {
                return invariantResult;
            }

            return null;
        }
    }
}