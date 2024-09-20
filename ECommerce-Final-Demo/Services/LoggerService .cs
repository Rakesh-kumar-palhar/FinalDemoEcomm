using ECommerce_Final_Demo.Model;

namespace ECommerce_Final_Demo.Services
{
    public interface ILoggerService
    {
        void Log(Exception ex);
    }
    public class LoggerService : ILoggerService
    {
        private readonly ApplicationDbContext _context;

        public LoggerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Log(Exception ex)
        {
            var log = new Logger
            {
                ExceptionType = ex.GetType().Name,
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            };

            _context.Loggers.Add(log);
            _context.SaveChanges();  // save log entry to database
        }
    }
}
