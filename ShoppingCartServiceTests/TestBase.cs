using AutoMapper;
using Moq.AutoMock;
using static ShoppingCartServiceTests.HelperExtensions;

namespace ShoppingCartServiceTests
{
    public class TestBase
    {
        protected readonly IMapper _mapper;

        protected readonly AutoMocker _mocker;

        public TestBase()
        {
            _mapper = ConfigureMapper();
            _mocker = new AutoMocker();
            _mocker.Use(_mapper);
        }
    }
}