namespace TableDBMS.Models
{
    public class Row
    {
        public List<string?> Values { get; set; } = new();

        public Row()
        {
        }

        public Row(IEnumerable<string?> values)
        {
            Values = values.ToList();
        }
    }
}