using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using HouseCom.Controllers;
using HouseCom.Models;
using HouseCom.Models.DTO;
using HouseCom.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseTester.Controller
{
    public class HouseControllerTest
    {
        private readonly IHouseRepository _context;
        private readonly IMapper _mapper;
        protected APIResponse _response;

        public HouseControllerTest()
        {
            _context = A.Fake<IHouseRepository>();
            _mapper = A.Fake<IMapper>();
            _response = new();
        }

        [Fact]
        public async Task HousesController_GetHouses_ReturnNotNullAsync()
        {
            // Arrange
            var houselist = A.Fake<IEnumerable<House>>(); 
            var houses = A.Fake<List<HouseDTO>>();
            A.CallTo(() => _mapper.Map<List<HouseDTO>>(houselist)).Returns(houses);
            var controller = new HousesController(_context, _mapper);
            //Act
            var result = await controller.GetHouses(1,"");

            //Assert
            result.Should().NotBeNull();
        }

    
    }
}
