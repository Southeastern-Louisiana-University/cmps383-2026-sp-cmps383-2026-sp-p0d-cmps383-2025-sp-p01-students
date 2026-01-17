using System.Net;
using FluentAssertions;
using Selu383.SP24.Tests.Dtos;
using Selu383.SP24.Tests.Helpers;

namespace Selu383.SP24.Tests.Controllers;

[TestClass]
public class TheatersControllerTests
{
    private WebTestContext context = new();

    [TestInitialize]
    public void Init()
    {
        context = new WebTestContext();
    }

    [TestCleanup]
    public void Cleanup()
    {
        context.Dispose();
    }

    [TestMethod]
    public async Task ListAllTheaters_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/theaters");

        //assert
        await httpResponse.AssertTheaterListAllFunctions();
    }

    [TestMethod]
    public async Task GetTheaterById_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetTheater();
        if (target == null)
        {
            Assert.Fail("Make List All theaters work first");
            return;
        }

        //act
        var httpResponse = await webClient.GetAsync($"/api/theaters/{target.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/theaters/{id} ");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<TheaterDto>();
        resultDto.Should().BeEquivalentTo(target, "we expect get product by id to return the same data as the list all product endpoint");
    }

    [TestMethod]
    public async Task GetTheaterById_NoSuchId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/theaters/999999");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling GET /api/theaters/{id} with an invalid id");
    }

    [TestMethod]
    public async Task CreateTheater_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Address = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/theaters with no name");
    }

    [TestMethod]
    public async Task CreateTheater_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a".PadLeft(121, '0'),
            Address = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/theaters with a name that is too long");
    }

    [TestMethod]
    public async Task CreateTheater_NoAddress_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetTheater();
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        var request = new TheaterDto
        {
            Name = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/theaters with no description");
    }

    [TestMethod]
    public async Task CreateTheater_Returns201AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);

        //assert
        await httpResponse.AssertCreateTheaterFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateTheater_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        request.Name = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/theaters/{id} with a missing name");
    }

    [TestMethod]
    public async Task UpdateTheater_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        request.Name = "a".PadLeft(121, '0');

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/theaters/{id} with a name that is too long");
    }

    [TestMethod]
    public async Task UpdateTheater_NoAddress_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        request.Address = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/theaters/{id} with a missing description");
    }

    [TestMethod]
    public async Task UpdateTheater_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        request.Address = "cool new description";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        await httpResponse.AssertTheaterUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task DeleteTheater_NoSuchItem_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Address = "asd",
            Name = "asd"
        };
        await using var itemHandle = await webClient.CreateTheater(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/theaters/{request.Id + 21}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/theaters/{id} with an invalid Id");
    }

    [TestMethod]
    public async Task DeleteTheater_ValidItem_ReturnsOk()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Address = "asd",
            Name = "asd",
        };
        await using var itemHandle = await webClient.CreateTheater(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/theaters/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/theaters/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteTheater_SameItemTwice_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Address = "asd",
            Name = "asd",
        };
        await using var itemHandle = await webClient.CreateTheater(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        await webClient.DeleteAsync($"/api/theaters/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/theaters/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/theaters/{id} on the same item twice");
    }
}
