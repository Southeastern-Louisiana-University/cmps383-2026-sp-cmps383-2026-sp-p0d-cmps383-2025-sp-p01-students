using System.Net;
using FluentAssertions;
using Selu383.SP24.Tests.Dtos;
using Selu383.SP24.Tests.Helpers;

namespace Selu383.SP24.Tests.Controllers;

internal static class TheatersHelpers
{
    internal static async Task<IAsyncDisposable?> CreateTheater(this HttpClient webClient, TheaterDto request)
    {
        try
        {
            var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);
            var resultDto = await AssertCreateTheaterFunctions(httpResponse, request, webClient);
            request.Id = resultDto.Id;
            return new DeleteTheater(resultDto, webClient);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static async Task<List<TheaterDto>?> GetTheaters(this HttpClient webClient)
    {
        try
        {
            var getAllRequest = await webClient.GetAsync("/api/theaters");
            var getAllResult = await AssertTheaterListAllFunctions(getAllRequest);
            return getAllResult.ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static async Task<TheaterDto?> GetTheater(this HttpClient webClient)
    {
        try
        {
            var getAllRequest = await webClient.GetAsync("/api/theaters");
            var getAllResult = await AssertTheaterListAllFunctions(getAllRequest);
            return getAllResult.OrderByDescending(x => x.Id).First();
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static async Task AssertTheaterUpdateFunctions(this HttpResponseMessage httpResponse, TheaterDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling PUT /api/theaters/{id} with valid data to update a theater");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<TheaterDto>();
        resultDto.Should().BeEquivalentTo(request, "We expect the update theater endpoint to return the result");

        var getByIdResult = await webClient.GetAsync($"/api/theaters/{request.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the updated theater by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<TheaterDto>();
        dtoById.Should().BeEquivalentTo(request, "we expect the same result to be returned by an update theater call as what you'd get from get theater by id");

        var getAllRequest = await webClient.GetAsync("/api/theaters");
        var listAllData =  await AssertTheaterListAllFunctions(getAllRequest);

        Assert.IsNotNull(listAllData, "We expect json data when calling GET /api/theaters");
        listAllData.Should().NotBeEmpty("list all should have something if we just updated a theater");
        var matchingItem = listAllData.Where(x => x.Id == request.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created theater by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(request, "we expect the same result to be returned by a updated theater as what you'd get from get getting all theaters");
    }

    internal static async Task<TheaterDto> AssertCreateTheaterFunctions(this HttpResponseMessage httpResponse, TheaterDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created, "we expect an HTTP 201 when calling POST /api/theaters with valid data to create a new theater");

        var resultDto = await httpResponse.Content.ReadAsJsonAsync<TheaterDto>();
        Assert.IsNotNull(resultDto, "We expect json data when calling POST /api/theaters");

        resultDto.Id.Should().BeGreaterOrEqualTo(1, "we expect a newly created theater to return with a positive Id");
        resultDto.Should().BeEquivalentTo(request, x => x.Excluding(y => y.Id), "We expect the create theater endpoint to return the result");

        httpResponse.Headers.Location.Should().NotBeNull("we expect the 'location' header to be set as part of a HTTP 201");
        httpResponse.Headers.Location.Should().Be($"http://localhost/api/theaters/{resultDto.Id}", "we expect the location header to point to the get theater by id endpoint");

        var getByIdResult = await webClient.GetAsync($"/api/theaters/{resultDto.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the newly created theater by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<TheaterDto>();
        dtoById.Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a create theater as what you'd get from get theater by id");

        var getAllRequest = await webClient.GetAsync("/api/theaters");
        var listAllData =  await AssertTheaterListAllFunctions(getAllRequest);

        Assert.IsNotNull(listAllData, "We expect json data when calling GET /api/theaters");
        listAllData.Should().NotBeEmpty("list all should have something if we just created a theater");
        var matchingItem = listAllData.Where(x => x.Id == resultDto.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created theater by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a created theater as what you'd get from get getting all theaters");

        return resultDto;
    }

    internal static async Task<List<TheaterDto>> AssertTheaterListAllFunctions(this HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/theaters");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<List<TheaterDto>>();
        Assert.IsNotNull(resultDto, "We expect json data when calling GET /api/theaters");
        resultDto.Should().HaveCountGreaterThan(2, "we expect at least 3 theaters when calling GET /api/theaters");
        resultDto.All(x => !string.IsNullOrWhiteSpace(x.Name)).Should().BeTrue("we expect all theaters to have names");
        resultDto.All(x => x.Id > 0).Should().BeTrue("we expect all theaters to have an id");
        var ids = resultDto.Select(x => x.Id).ToArray();
        ids.Should().HaveSameCount(ids.Distinct(), "we expect Id values to be unique for every theater");
        return resultDto;
    }

    private sealed class DeleteTheater : IAsyncDisposable
    {
        private readonly TheaterDto request;
        private readonly HttpClient webClient;

        public DeleteTheater(TheaterDto request, HttpClient webClient)
        {
            this.request = request;
            this.webClient = webClient;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await webClient.DeleteAsync($"/api/theaters/{request.Id}");
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
