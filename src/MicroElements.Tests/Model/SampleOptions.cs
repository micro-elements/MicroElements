namespace MicroElements.Tests.Model
{
    public class SampleOptions : ISampleOptions
    {
        public string Value { get; set; }

        public string SharedValue { get; set; }

        public int? OptionalIntValue { get; set; }

        public string OptionalValue { get; set; }
    }
}
