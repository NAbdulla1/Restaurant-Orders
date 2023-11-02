namespace RestaurantOrder.Core.Exceptions
{
    public class OrderVersionNotProvidedException : Exception
    {
        public OrderVersionNotProvidedException(string message) : base(message) { }
    }
}
