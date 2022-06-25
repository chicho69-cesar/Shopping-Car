using ShoppingCar.Data.Entities;

namespace ShoppingCar.Helpers {
    public interface IGetLocation {
        IOrderedEnumerable<State> GetStates(int countryId);
        IOrderedEnumerable<City> GetCities(int stateId);
    }
}