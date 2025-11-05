using ApiV1ControlleurMonstre.DTOs;
using ApiV1ControlleurMonstre.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace ApiV1ControlleurMonstre.Tests
{
    public class Tuiles : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public Tuiles(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

    }
}
