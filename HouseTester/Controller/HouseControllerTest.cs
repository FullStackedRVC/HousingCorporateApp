using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using HouseCom.Controllers;
using HouseCom.Models;
using HouseCom.Models.DTO;
using HouseCom.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xunit;


namespace HouseTester.Controller
{
    public class HouseControllerTest
    {
        private readonly IHouseRepository _context;
        private readonly IMapper _mapper;
        private readonly ILogger<HousesController> _logger;
        protected APIResponse _response;

        public HouseControllerTest()
        {
            _context = A.Fake<IHouseRepository>();
            _mapper = A.Fake<IMapper>();
            _logger = A.Fake<ILogger<HousesController>>();
            _response = new();
        }

        [Theory]
        [InlineData(1)]
        public async Task HousesController_GetHouse_ReturnsHouse_WhenExists(int id)
        {
            // Arrange  
            var house = new House { Id = id, Name = "Test House" };
            var houseDTO = new HouseDTO { Id = id, Name = "Test House" };
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, true, null)).Returns(house);
            A.CallTo(() => _mapper.Map<HouseDTO>(house)).Returns(houseDTO);
            var controller = new HousesController(_context, _mapper, _logger);

            // Act  
            var result = await controller.GetHouse(id);

            // Assert  
            result.Should().NotBeNull();
            result.Value.Should().BeOfType<APIResponse>();
            var apiResponse = result.Value as APIResponse;
            apiResponse.Result.Should().Be(houseDTO);
            apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData(99)]
        public async Task HousesController_GetHouse_ReturnsNotFound_WhenHouseDoesNotExist(int id)
        {
            // Arrange
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, true, null)).Returns(Task.FromResult<House?>(null));
            var controller = new HousesController(_context, _mapper, _logger);

            // Act
            var result = await controller.GetHouse(id);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task HousesController_GetAllHouses_ReturnsListOfHouses()
        {
            // Arrange
            var houses = new List<House> { new House { Id = 1, Name = "A" }, new House { Id = 2, Name = "B" } };
            var houseDTOs = new List<HouseDTO> { new HouseDTO { Id = 1, Name = "A" }, new HouseDTO { Id = 2, Name = "B" } };
            A.CallTo(() => _context.GetAllHouses(null, null, 0, 1)).Returns(houses);
            A.CallTo(() => _mapper.Map<List<HouseDTO>>(houses)).Returns(houseDTOs);
            var controller = new HousesController(_context, _mapper, _logger);

            // Act
            var result = await controller.GetHouses(null, null);
            

            // No additional changes are needed if the HouseUpdateDTO class exists in the HouseCom.Models.DTO namespace.
            // Assert
            result.Should().NotBeNull();
            result.Value.Should().BeOfType<APIResponse>();
            var apiResponse = result.Value as APIResponse;
            apiResponse.Result.Should().Be(houseDTOs);
            apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task HousesController_PostHouse_ReturnsCreated_WhenValid()
        {
            // Arrange
            var houseDTO = new HouseCreateDTO { Name = "New House" };
            var house = new House { Id = 1, Name = "New House" };
            A.CallTo(() => _mapper.Map<House>(houseDTO)).Returns(house);
            A.CallTo(() => _context.CreateHouse(house)).Returns(Task.CompletedTask);
            var controller = new HousesController(_context, _mapper, _logger);

            // Act
            var result = await controller.PostHouse(houseDTO);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.Created);
        }

        [Fact]
        public async Task HousesController_PostHouse_ReturnsBadRequest_WhenInputIsNull()
        {
            // Arrange
            var controller = new HousesController(_context, _mapper, _logger);

            // Act
            var result = await controller.PostHouse(null);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task HousesController_PutHouse_ReturnsNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            var id = 1;
            var houseDTO = new HouseDTO { Name = "Updated House" };
            var house = new House { Id = id, Name = "Updated House" };
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, true, null)).Returns(house);
            A.CallTo(() => _mapper.Map<House>(houseDTO)).Returns(house);
            A.CallTo(() => _context.UpdateHouse(house)).Returns(Task.CompletedTask);
            var controller = new HousesController(_context, _mapper, _logger);

            // Act
            var result = await controller.PutHouse(id, houseDTO);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task HousesController_PutHouse_ReturnsNotFound_WhenHouseDoesNotExist()
        {
            // Arrange
            var id = 99;
            var houseDTO = new HouseDTO { Name = "Updated House" };
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, true, null)).Returns(Task.FromResult<House?>(null));
            var controller = new HousesController(_context, _mapper, _logger);

            // Act
            var result = await controller.PutHouse(id, houseDTO);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task HousesController_DeleteHouse_ReturnsNoContent_WhenDeleteSucceeds()
        {
            // Arrange
            var id = 1;
            var house = new House { Id = id, Name = "To Delete" };
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, true, null)).Returns(house);
            A.CallTo(() => _context.DeleteHouse(house)).Returns(Task.CompletedTask);
            var controller = new HousesController(_context, _mapper, _logger);

            // Act
            var result = await controller.DeleteHouse(id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task HousesController_DeleteHouse_ReturnsNotFound_WhenHouseDoesNotExist()
        {
            // Arrange
            var id = 99;
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, true, null)).Returns(Task.FromResult<House?>(null));
            var controller = new HousesController(_context, _mapper, _logger);

            // Act
            var result = await controller.DeleteHouse(id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}