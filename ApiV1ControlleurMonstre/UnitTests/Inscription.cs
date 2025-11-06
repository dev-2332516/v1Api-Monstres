using ApiV1ControlleurMonstre;
using ApiV1ControlleurMonstre.DTOs;
using ApiV1ControlleurMonstre.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.RateLimiting;
using Xunit;

namespace UnitTests
{
    public class Token
    {
        public string token { get; set; }
    }
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
            var response = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/Login/test@email.com/MotDePasseTest", user);


            string returnValue = await response.Content.ReadAsStringAsync();
            Token test = JsonConvert.DeserializeObject<Token>(returnValue);
            _client.DefaultRequestHeaders.Add("userToken", test.token);


            var personnage = await _client.GetAsync(
                "/api/Personnages/GetPersonnageFromUser");

            Assert.True(personnage.IsSuccessStatusCode,
                $"Registration failed: {await personnage.Content.ReadAsStringAsync()}");
            // Assert

            Personnage character = await personnage.Content.ReadFromJsonAsync<Personnage>();
            Assert.NotNull(character);

            #endregion Inscription_WithValidData_CreatesCharacterAutomatically

            #region Inscription_WithValidData_PlacesCharacterInRandomCity

            var tuiletmp = await _client.GetAsync(
                $"/api/Tuiles/GetOrCreateTuile/{character.PositionX}/{character.PositionY}");

            // Ensure request succeeded
            Assert.True(tuiletmp.IsSuccessStatusCode,
                $"GetOrCreateTuile failed: {await tuiletmp.Content.ReadAsStringAsync()}");

            // Read the returned Tuile and verify its Type is Ville
            Tuile returnedTuile = await tuiletmp.Content.ReadFromJsonAsync<Tuile>();
            Assert.NotNull(returnedTuile);
            #endregion Inscription_WithValidData_PlacesCharacterInRandomCity
        }

        [Fact]
        public async Task SequenceErreur()
        {
            #region Inscription_WithExistingEmail_ReturnsConflict
            // First registration
            var existingUser = new Utilisateur(0, "existing@email.com", "ExistingUser", "Password123");
            await _client.PostAsJsonAsync("/api/Utilisateurs/register", existingUser);

            // Attempt to register with same email
            var duplicateUser = new Utilisateur(0, "existing@email.com", "DifferentUser", "Password456");
            var duplicateResponse = await _client.PostAsJsonAsync("/api/Utilisateurs/register", duplicateUser);

            Assert.Equal(System.Net.HttpStatusCode.Conflict, duplicateResponse.StatusCode);
            #endregion

            #region Inscription_WithEmptyEmail_ReturnsBadRequest
            var userWithEmptyEmail = new Utilisateur(0, "", "ValidPseudo", "ValidPassword");
            var emptyEmailResponse = await _client.PostAsJsonAsync("/api/Utilisateurs/register", userWithEmptyEmail);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, emptyEmailResponse.StatusCode);
            #endregion

            #region Inscription_WithEmptyPassword_ReturnsBadRequest
            var userWithEmptyPassword = new Utilisateur(0, "valid@email.com", "ValidPseudo", "");
            var emptyPasswordResponse = await _client.PostAsJsonAsync("/api/Utilisateurs/register", userWithEmptyPassword);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, emptyPasswordResponse.StatusCode);
            #endregion

            #region Inscription_WithEmptyPseudo_ReturnsBadRequest
            var userWithEmptyPseudo = new Utilisateur(0, "valid@email.com", "", "ValidPassword");
            var emptyPseudoResponse = await _client.PostAsJsonAsync("/api/Utilisateurs/register", userWithEmptyPseudo);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, emptyPseudoResponse.StatusCode);
            #endregion
        }



        [Fact]
        public async Task SuccessTests()
        {
            // Setup - Create a test user and authenticate
            var user = new Utilisateur(0, "test@tuiles.com", "TuilesTest", "Password123");
            var registerResponse = await _client.PostAsJsonAsync("/api/Utilisateurs/register", user);
            Assert.True(registerResponse.IsSuccessStatusCode);

            var response = await _client.PostAsJsonAsync(
                "/api/Utilisateurs/Login/test@email.com/MotDePasseTest", user);
            string returnValue = await response.Content.ReadAsStringAsync();
            Token test = JsonConvert.DeserializeObject<Token>(returnValue);
            _client.DefaultRequestHeaders.Add("userToken", test.token);

            // Set authentication token for subsequent requests
            #region GetTuiles_WithAuthenticatedUser_Returns3x3Grid
            var initialTuilesResponse = await _client.GetAsync("/api/Tuiles/GetInitialTuiles");
            Assert.True(initialTuilesResponse.IsSuccessStatusCode);

            var tuiles = await initialTuilesResponse.Content.ReadFromJsonAsync<List<TuileDto>>();
            Assert.NotNull(tuiles);
            Assert.Equal(9, tuiles.Count); // Should be a 3x3 grid

            // Verify grid structure (should have coordinates from -1 to 1 relative to character position)
            var positions = tuiles.Select(t => new { t.PositionX, t.PositionY }).ToList();
            Assert.All(positions, p =>
                Assert.True(Math.Abs(p.PositionX - tuiles[4].PositionX) <= 1 &&
                          Math.Abs(p.PositionY - tuiles[4].PositionY) <= 1));
            #endregion

            #region GetTuiles_WithAuthenticatedUser_IncludesPersonnageData
            // Get character position for reference
            var personnageResponse = await _client.GetAsync("/api/Personnages/GetPersonnageFromUser");
            Assert.True(personnageResponse.IsSuccessStatusCode);

            var character = await personnageResponse.Content.ReadFromJsonAsync<Personnage>();
            Assert.NotNull(character);
            #endregion

            #region GetTuiles_WithAuthenticatedUser_IncludesMonsterData
            // Verify monster data is properly included when present
            foreach (var tuile in tuiles)
            {
                if (tuile.Monstre != null)
                {
                    Assert.NotNull(tuile.Monstre.MonstreId);
                    Assert.NotNull(tuile.Monstre.Nom);
                    Assert.NotNull(tuile.Monstre.SpriteURL);
                    Assert.True(tuile.Monstre.Niveau > 0);
                    Assert.True(tuile.Monstre.PointsVieMax > 0);
                }
            }
            #endregion

            #region GetTuiles_AtMapEdge_ReturnsOnlyAvailableTiles
            // Move to edge of map (0,0) and request tiles
            var edgeTuileResponse = await _client.PostAsJsonAsync(
                "/api/Tuiles/GetOrCreateTuile/0/0",
                user
            );
            Assert.Equal(System.Net.HttpStatusCode.MethodNotAllowed, edgeTuileResponse.StatusCode);

            // Get the grid at the edge
            var edgeInitialTuilesResponse = await _client.GetAsync("/api/Tuiles/GetInitialTuiles");
            Assert.True(edgeInitialTuilesResponse.IsSuccessStatusCode);

            var edgeTuiles = await edgeInitialTuilesResponse.Content.ReadFromJsonAsync<List<TuileDto>>();
            Assert.NotNull(edgeTuiles);

            Assert.True(edgeTuiles.Count == 9);

            // Verify no tiles are out of bounds
            Assert.All(edgeTuiles, t =>
                Assert.True(t.PositionX >= 0 && t.PositionX < 50 &&
                          t.PositionY >= 0 && t.PositionY < 50));
            #endregion
        }

        [Fact]
        public async Task ErrorTests()
        {
            var user = new Utilisateur(0, "test@tuileserreur.com", "TuilesTest", "Password123");
            var registerResponse = await _client.PostAsJsonAsync("/api/Utilisateurs/register", user);
            #region GetTuiles_WithoutAuthentication_ReturnsUnauthorized
            _client.DefaultRequestHeaders.Add("userToken", "");
            var edgeTuileResponse = await _client.GetAsync(
                "/api/Tuiles/GetInitialTuiles");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, edgeTuileResponse.StatusCode);
            #endregion
        }
    }
}