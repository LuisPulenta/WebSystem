using WebSystem.Shared.Responses;
namespace WebSystem.API.Helpers
{
    public interface IMailHelper
    {
        Response SendMail(string toName, string toEmail, string subject, string body);
    }
}
