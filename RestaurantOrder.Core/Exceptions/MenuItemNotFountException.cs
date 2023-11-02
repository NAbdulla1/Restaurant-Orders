namespace RestaurantOrder.Core.Exceptions
{
    public class MenuItemNotFountException : Exception
    {
        public MenuItemNotFountException(string message) : base(message) { }
        public MenuItemNotFountException() : base() { }
    }
}
