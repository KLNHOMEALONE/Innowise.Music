namespace Innowise.Music.Validations.Rules
{
    public class CompareRule<T> : IValidationRule<T>
    {
        private readonly Func<T> _valueToCompare;
        public string ValidationMessage { get; set; }

        public CompareRule(Func<T> valueToCompare)
        {
            _valueToCompare = valueToCompare;
        }

        public bool Check(T value)
        {
            return Equals(value, _valueToCompare());
        }
    }
}
