namespace Restaurant_Orders.Exceptions
{
    public class MenuItemDoesNotExists : Exception
    {
        public MenuItemDoesNotExists(string message) : base(message) { }
    }
}
