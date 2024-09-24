namespace ECommerce_Final_Demo.Model
{
    public class ChangePassword
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
