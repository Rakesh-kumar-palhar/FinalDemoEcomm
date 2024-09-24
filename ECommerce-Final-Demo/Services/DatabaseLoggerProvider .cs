using ECommerce_Final_Demo.Model;

namespace ECommerce_Final_Demo.Services
{
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DatabaseLoggerProvider(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public ILogger CreateLogger(string categoryName)
        {
            // Use a factory method to create the DbContext when logging
            Func<ApplicationDbContext> dbContextFactory = () =>
            {
                var scope = _serviceScopeFactory.CreateScope();
                return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            };

            return new DatabaseLogger(categoryName, dbContextFactory);
        }

        public void Dispose()
        {
            // No resources to dispose for now
        }
    }


}
