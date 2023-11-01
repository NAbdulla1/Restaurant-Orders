namespace RestaurantOrder.Core.Exceptions
{
    public class UnauthenticatedException : Exception
    {
        public UnauthenticatedException(string msg) : base(msg) { }
    }
}
