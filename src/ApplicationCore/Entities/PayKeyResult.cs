namespace ApplicationCore.Entities
{
    public class PayKeyResult
    {
        public string Value { get; }
        public int Collisions { get; }

        public PayKeyResult(string value, int collisions)
        {
            Value = value;
            Collisions = collisions;
        }
    }
}
