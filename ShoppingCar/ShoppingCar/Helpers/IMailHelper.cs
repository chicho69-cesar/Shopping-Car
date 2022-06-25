using ShoppingCar.Common;

namespace ShoppingCar.Helpers {
    public interface IMailHelper {
        Response SendMail(string toName, string toEmail, string subject, string body);
    }
}