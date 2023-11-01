namespace RestaurantOrder.Core.Exceptions
{
    public class CustomerAlreadyExistsException : Exception
    {
        public CustomerAlreadyExistsException(string? message) : base(message) { }
    }
}
