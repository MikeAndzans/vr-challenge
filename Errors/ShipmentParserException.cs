namespace vr_challenge.Errors
{
    public class ShipmentParserException : IOException
    {
        public ShipmentParserException() { }

        public ShipmentParserException(string message) : base(message) { }

        public ShipmentParserException(string message, Exception inner) : base(message, inner) { }
    }
}
