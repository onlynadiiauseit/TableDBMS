namespace TableDBMS.DataTypes
{
    public static class DataTypeFactory
    {
        public static IDataType Create(
            string typeName)
        {
            return typeName switch
            {
                "Integer" =>
                    new IntegerType(),

                "Real" =>
                    new RealType(),

                "Char" =>
                    new CharType(),

                "String" =>
                    new StringType(),

                "Color" =>
                    new ColorType(),

                "ColorInvl" =>
                    new ColorIntervalType(),

                _ => throw new ArgumentException(
                    $"Unknown data type: {typeName}"
                )
            };
        }

        public static string[] GetAvailableTypes()
        {
            return
            [
                "Integer",
                "Real",
                "Char",
                "String",
                "Color",
                "ColorInvl"
            ];
        }
    }
}