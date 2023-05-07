namespace StegSharp.Application.Common.Exceptions
{
    public class CapacityException : Exception
    {
        public double MessageBitLength { get; set; }

        public double AvailableCoefficientCount { get; set; }

        public CapacityException() { }

        public CapacityException(string message) : base(message) { }

        public CapacityException(string message, double availableCoefficientCount, double messageBitLength) : base(message) 
        {
            this.AvailableCoefficientCount = availableCoefficientCount;
            this.MessageBitLength = messageBitLength;
        }

        public CapacityException(string message, Exception innerException) : base(message, innerException) { }
    }
}
