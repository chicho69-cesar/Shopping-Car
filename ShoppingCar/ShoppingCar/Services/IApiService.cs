using ShoppingCar.Common;

namespace ShoppingCar.Services {
    public interface IApiService {
        Task<Response> GetListAsync<T>(string servicePrefix, string controller);
    }
}