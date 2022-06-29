using ShoppingCar.Common;
using ShoppingCar.Models;

namespace ShoppingCar.Helpers {
    public interface IOrdersHelper {
        Task<Response> ProcessOrderAsync(ShowCartViewModel model);
        Task<Response> CancelOrderAsync(int id);
    }
}