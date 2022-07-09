using AutoMapper;
using ShoppingCar.Data.Entities;
using ShoppingCar.Models;

namespace ShoppingCar.Services {
    public class AutoMapperProfile : Profile {
        public AutoMapperProfile() {
            CreateMap<User, EditUserViewModel>().ReverseMap();
            CreateMap<Product, AddProductToCartViewModel>().ReverseMap();
        }
    }
}