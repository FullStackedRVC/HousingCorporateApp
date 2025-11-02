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

        private HousesController CreateController() => new HousesController(_context, _mapper, _logger);

        [Theory]
        [InlineData(1)]
        public async Task HousesController_GetHouse_ReturnsHouse_WhenExists(int id)
        {
            // Arrange  
            var house = new House { Id = id, Name = "Test House" };
            var houseDTO = new HouseDTO { Id = id, Name = "Test House" };
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, A<bool>._, A<string?>._))
                .Returns(Task.FromResult<House?>(house));
            // Use argument matcher so test doesn't depend on reference equality of the instance
            A.CallTo(() => _mapper.Map<HouseDTO>(A<House>._)).Returns(houseDTO);
            var controller = CreateController();

            // Act  
            var result = await controller.GetHouse(id);

            // Assert  
            result.Should().NotBeNull();
            result.Value.Should().BeOfType<APIResponse>();
            var apiResponse = result.Value as APIResponse;
            apiResponse.Should().NotBeNull();
            // Compare by value not reference
            apiResponse!.Result.Should().BeEquivalentTo(houseDTO);
            apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            // verify mapper was used
            A.CallTo(() => _mapper.Map<HouseDTO>(A<House>._)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(99)]
        public async Task HousesController_GetHouse_ReturnsNotFound_WhenHouseDoesNotExist(int id)
        {
            // Arrange
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, A<bool>._, A<string?>._))
                .Returns(Task.FromResult<House?>(null));
            var controller = CreateController();

            // Act
            var result = await controller.GetHouse(id);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task HousesController_GetAllHouses_ReturnsListOfHouses()
        {
            // Arrange
            var houses = new List<House> { new House { Id = 1, Name = "A" }, new House { Id = 2, Name = "B" } };
            var houseDTOs = new List<HouseDTO> { new HouseDTO { Id = 1, Name = "A" }, new HouseDTO { Id = 2, Name = "B" } };
            A.CallTo(() => _context.GetAllHouses(A<Expression<Func<House, bool>>>._, A<string?>._, A<int>._, A<int>._))
                .Returns(Task.FromResult<IEnumerable<House>>(houses));
            A.CallTo(() => _mapper.Map<List<HouseDTO>>(A<IEnumerable<House>>._)).Returns(houseDTOs);
            var controller = CreateController();

            // Act
            var result = await controller.GetHouses(null, null);

            // Assert
            result.Should().NotBeNull();
            result.Value.Should().BeOfType<APIResponse>();
            var apiResponse = result.Value as APIResponse;
            apiResponse.Should().NotBeNull();
            apiResponse!.Result.Should().BeEquivalentTo(houseDTOs);
            apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            A.CallTo(() => _mapper.Map<List<HouseDTO>>(A<IEnumerable<House>>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task HousesController_PostHouse_ReturnsCreated_WhenValid()
        {
            // Arrange
            var houseDTO = new HouseCreateDTO { Name = "New House" };
            var house = new House { Id = 1, Name = "New House" };
            A.CallTo(() => _mapper.Map<House>(A<HouseCreateDTO>._)).Returns(house);
            A.CallTo(() => _context.CreateHouse(A<House>._)).Returns(Task.CompletedTask);
            var controller = CreateController();

            // Act
            var result = await controller.PostHouse(houseDTO);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            // verify repository was called to create
            A.CallTo(() => _context.CreateHouse(A<House>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task HousesController_PostHouse_ReturnsBadRequest_WhenInputIsNull()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.PostHouse(null!);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            // should not try to create anything when input is null
            A.CallTo(() => _context.CreateHouse(A<House>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task HousesController_PutHouse_ReturnsNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            var id = 1;
            var houseDTO = new HouseDTO { Name = "Updated House" };
            var house = new House { Id = id, Name = "Updated House" };
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, A<bool>._, A<string?>._))
                .Returns(Task.FromResult<House?>(house));
            A.CallTo(() => _mapper.Map<House>(A<HouseDTO>._)).Returns(house);
            A.CallTo(() => _context.UpdateHouse(A<House>._)).Returns(Task.CompletedTask);
            var controller = CreateController();

            // Act
            var result = await controller.PutHouse(id, houseDTO);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            A.CallTo(() => _context.UpdateHouse(A<House>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task HousesController_PutHouse_ReturnsNotFound_WhenHouseDoesNotExist()
        {
            // Arrange
            var id = 99;
            var houseDTO = new HouseDTO { Name = "Updated House" };
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, A<bool>._, A<string?>._))
                .Returns(Task.FromResult<House?>(null));
            var controller = CreateController();

            // Act
            var result = await controller.PutHouse(id, houseDTO);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            A.CallTo(() => _context.UpdateHouse(A<House>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task HousesController_DeleteHouse_ReturnsNoContent_WhenDeleteSucceeds()
        {
            // Arrange
            var id = 1;
            var house = new House { Id = id, Name = "To Delete" };
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, A<bool>._, A<string?>._))
                .Returns(Task.FromResult<House?>(house));
            A.CallTo(() => _context.DeleteHouse(A<House>._)).Returns(Task.CompletedTask);
            var controller = CreateController();

            // Act
            var result = await controller.DeleteHouse(id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            A.CallTo(() => _context.DeleteHouse(A<House>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task HousesController_DeleteHouse_ReturnsNotFound_WhenHouseDoesNotExist()
        {
            // Arrange
            var id = 99;
            A.CallTo(() => _context.GetHouse(A<Expression<Func<House, bool>>>._, A<bool>._, A<string?>._))
                .Returns(Task.FromResult<House?>(null));
            var controller = CreateController();

            // Act
            var result = await controller.DeleteHouse(id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            A.CallTo(() => _context.DeleteHouse(A<House>._)).MustNotHaveHappened();
        }
    }
}