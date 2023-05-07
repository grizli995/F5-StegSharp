namespace StegSharp.Application.Common.Exceptions
{
    public class CapacityException : Exception
    {
        public CapacityException() { }

        public CapacityException(string message) : base(message) { }

        public CapacityException(string message, Exception innerException) : base(message, innerException) { }
    }
}
