namespace AAARunCheck.Config
{
    public class ValueRow
    {
        public ValueWrapper[] Values { get; set; }

        public ValueRow(int size)
        {
            Values = new ValueWrapper[size];
        }

        public override string ToString()
        {
            return $"{nameof(Values)}: {Values}";
        }
    }
}