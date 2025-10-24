using ApiV1ControlleurMonstre.Data.DTOs;
using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UnitTests
{
    public class Tuiles : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SimpleGameFlowTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
    {
        [Fact]
        public void Inscription_WithValidData_ReturnsCreated()
        {

        }
    }
}
