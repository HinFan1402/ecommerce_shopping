namespace ecommerce_shopping.Areas.Admin.Repository
{
    public interface IEmailSender
    {
        Task SenderEmailAsync(string email, string subject, string message);
    }
}
