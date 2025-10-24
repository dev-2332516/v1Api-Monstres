using ApiV1ControlleurMonstre;
using ApiV1ControlleurMonstre.DTOs;
using ApiV1ControlleurMonstre.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace UnitTests
{
    public class Inscription : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public Inscription(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }


        [Fact]
        public async void SequenceSuccess()
        {
            #region Inscription_WithValidData_ReturnsCreated
            Utilisateur user = new Utilisateur(0, "test@email.com", "PseudoTest", "MotDePasseTest");
            var registerResponse = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/register",
                user
            );

            // Verify registration succeeded
            Assert.True(registerResponse.IsSuccessStatusCode,
                $"Registration failed: {await registerResponse.Content.ReadAsStringAsync()}");
            #endregion Inscription_WithValidData_ReturnsCreated

            #region Inscription_WithValidData_CreatesCharacterAutomatically
            // Create base data
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", user.Token);
            var emptyData = new { Foo = "Bar" };

            var personnage = await _client.PostAsJsonAsync(
                "/api/Personnages/GetPersonnageFromUser", emptyData);

            // Assert
            Assert.True(personnage.IsSuccessStatusCode,
                $"Registration failed: {await personnage.Content.ReadAsStringAsync()}");

            Personnage character = await personnage.Content.ReadFromJsonAsync<Personnage>();
            Assert.NotNull(character);

            #endregion Inscription_WithValidData_CreatesCharacterAutomatically

            #region Inscription_WithValidData_PlacesCharacterInRandomCity

            #endregion Inscription_WithValidData_PlacesCharacterInRandomCity

        }
    }
}