namespace ShoppingCartService.Config
{
    public interface IShoppingCartDatabaseSettings
    {
        string ConnectionString { get; }
        string DatabaseName { get; }
    }
}