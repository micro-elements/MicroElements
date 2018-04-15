namespace MicroElements.Tests.Model
{
    public class ComplexObject
    {
        public string Name { get; set; }
        public InnerObject Inner { get; set; }
    }

    public class InnerObject
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class ComplexObjectContainer
    {
        public ComplexObject ComplexObject { get; set; }
    }
}
