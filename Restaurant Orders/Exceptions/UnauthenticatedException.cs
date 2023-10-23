namespace Restaurant_Orders.Exceptions
{
    public class UnauthenticatedException : Exception
    {
        public UnauthenticatedException(string msg) : base(msg) { }
    }
}
