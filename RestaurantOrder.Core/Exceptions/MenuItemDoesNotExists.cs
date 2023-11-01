namespace RestaurantOrder.Core.Exceptions
{
    public class MenuItemDoesNotExists : Exception
    {
        public MenuItemDoesNotExists(string message) : base(message) { }
        public MenuItemDoesNotExists() : base() { }
    }
}
