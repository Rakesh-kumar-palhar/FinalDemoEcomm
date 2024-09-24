using ECommerce_Final_Demo.Model;
using Microsoft.EntityFrameworkCore.Internal;

namespace ECommerce_Final_Demo.Services
{
    public class DatabaseLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Func<ApplicationDbContext> _dbContextFactory;

        public DatabaseLogger(string categoryName, Func<ApplicationDbContext> dbContextFactory)
        {
            _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Error; // Adjust as necessary
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception == null) return;

            // Create a new DbContext instance when logging
            using (var context = _dbContextFactory())
            {
                var logEntry = new Logger
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    ExceptionType = exception?.ToString(),
                   
                };

                context.Loggers.Add(logEntry);
                context.SaveChanges();
            }
        }
    }

}
