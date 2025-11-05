using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ApiV1ControlleurMonstre.Models;
using ApiV1ControlleurMonstre;
using FluentAssertions;
using Xunit;
using System.Text;
using System.Text.Json;

namespace UnitTests
{
    /// <summary>
    /// Tests d'intégration pour les fonctionnalités de connexion
    /// </summary>
    public class Connexion : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public Connexion(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region TESTS CONNEXION - SUCCÈS

        [Fact]
        public async Task Connexion_WithValidCredentials_ReturnsOk()
        {
            // Arrange
            var testEmail = $"login_{Guid.NewGuid()}@example.com";
            var password = "password123";
            var utilisateur = new Utilisateur
            {
                Email = testEmail,
                Pseudo = "LoginUser",
                Password = password
            };

            // Créer l'utilisateur d'abord
            await _client.PostAsJsonAsync("/api/utilisateurs/register", utilisateur);

            // Act
            var loginResponse = await _client.PostAsync($"/api/utilisateurs/login/{testEmail}/{password}", null);

            // Assert
            loginResponse.IsSuccessStatusCode.Should().BeTrue(
                $"Login failed: {await loginResponse.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Connexion_WithValidCredentials_SetsEstConnecteToTrue()
        {
            // Arrange
            var testEmail = $"login2_{Guid.NewGuid()}@example.com";
            var password = "password123";
            var utilisateur = new Utilisateur
            {
                Email = testEmail,
                Pseudo = "LoginUser2",
                Password = password
            };

            // Créer l'utilisateur d'abord
            await _client.PostAsJsonAsync("/api/utilisateurs/register", utilisateur);

            // Act
            var loginResponse = await _client.PostAsync($"/api/utilisateurs/login/{testEmail}/{password}", null);

            // Assert
            loginResponse.IsSuccessStatusCode.Should().BeTrue();
            var responseContent = await loginResponse.Content.ReadAsStringAsync();
            responseContent.ToLower().Should().Contain("token");
        }

        [Fact]
        public async Task Connexion_WithValidCredentials_AllowsSubsequentAuthenticatedRequests()
        {
            // Arrange
            var testEmail = $"login3_{Guid.NewGuid()}@example.com";
            var password = "password123";
            var utilisateur = new Utilisateur
            {
                Email = testEmail,
                Pseudo = "LoginUser3",
                Password = password
            };

            // Créer l'utilisateur d'abord
            var registerResponse = await _client.PostAsJsonAsync("/api/utilisateurs/register", utilisateur);
            Assert.True(registerResponse.IsSuccessStatusCode);

            // Login et récupérer le token
            var loginResponse = await _client.PostAsync($"/api/utilisateurs/login/{testEmail}/{password}", null);
            Assert.True(loginResponse.IsSuccessStatusCode);

            // Extraire le token de la réponse JSON
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent);
            var token = loginResult.GetProperty("token").GetString();
            token.Should().NotBeNullOrEmpty();

            // Act - Tester une requête authentifiée avec le token JWT
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/Personnages/GetPersonnageFromUser");
            request.Headers.Add("userToken", token);
            
            var response = await _client.SendAsync(request);
            
            // Assert
            response.IsSuccessStatusCode.Should().BeTrue(
                $"Authenticated request failed: {await response.Content.ReadAsStringAsync()}");

            // Vérifier que la réponse contient les données du personnage
            var personnageContent = await response.Content.ReadAsStringAsync();
            personnageContent.Should().NotBeNullOrEmpty();
            
            // Vérifier que c'est bien du JSON valide contenant les propriétés attendues
            var personnageData = JsonSerializer.Deserialize<JsonElement>(personnageContent);
            Assert.True(personnageData.TryGetProperty("name", out _) || 
                       personnageData.TryGetProperty("id", out _), 
                       "La réponse devrait contenir des informations sur le personnage");
        }

        #endregion

        #region TESTS CONNEXION - ERREURS

        [Fact]
        public async Task Connexion_WithInvalidEmail_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.PostAsync("/api/utilisateurs/login/nonexistent@example.com/password123", null);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("InvalidEmailPassword", content);
        }

        [Fact]
        public async Task Connexion_WithInvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var testEmail = $"wrongpass_{Guid.NewGuid()}@example.com";
            var correctPassword = "password123";
            var wrongPassword = "wrongpassword";
            
            var utilisateur = new Utilisateur
            {
                Email = testEmail,
                Pseudo = "WrongPassUser",
                Password = correctPassword
            };

            // Créer l'utilisateur d'abord
            await _client.PostAsJsonAsync("/api/utilisateurs/register", utilisateur);

            // Act
            var response = await _client.PostAsync($"/api/utilisateurs/login/{testEmail}/{wrongPassword}", null);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("InvalidEmailPassword", content);
        }

        [Fact]
        public async Task Connexion_WithNonexistentUser_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.PostAsync("/api/utilisateurs/login/doesnotexist@example.com/anypassword", null);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("InvalidEmailPassword", content);
        }

        [Fact]
        public async Task Connexion_WithEmptyCredentials_ReturnsBadRequest()
        {
            // Act & Assert - Email vide
            var responseEmptyEmail = await _client.PostAsync("/api/utilisateurs/login//password123", null);
            Assert.False(responseEmptyEmail.IsSuccessStatusCode);

            // Act & Assert - Mot de passe vide  
            var responseEmptyPassword = await _client.PostAsync($"/api/utilisateurs/login/test@example.com/", null);
            Assert.False(responseEmptyPassword.IsSuccessStatusCode);

            // Act & Assert - Les deux vides
            var responseBothEmpty = await _client.PostAsync("/api/utilisateurs/login//", null);
            Assert.False(responseBothEmpty.IsSuccessStatusCode);
        }

        #endregion
    }
}
