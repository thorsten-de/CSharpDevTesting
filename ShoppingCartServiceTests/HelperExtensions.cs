using AutoMapper;
using ShoppingCartService.Mapping;

namespace ShoppingCartServiceTests
{
    internal static class HelperExtensions
    {
        public static IMapper ConfigureMapper() =>
            new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()))
                .CreateMapper();
    }
}
