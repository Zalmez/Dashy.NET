using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Dashy.Net.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WeatherController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(IHttpClientFactory httpClientFactory, ILogger<WeatherController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetWeather(
            [FromQuery] double latitude = 59.91,
            [FromQuery] double longitude = 10.75,
            [FromQuery] string unit = "celsius")
        {
            var client = _httpClientFactory.CreateClient("WeatherApi");
            var url = $"v1/forecast?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&current_weather=true&temperature_unit={unit}";

            _logger.LogInformation("Attempting to fetch weather from Open-Meteo.");

            try
            {
                var responseMessage = await client.GetAsync(url);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    var errorBody = await responseMessage.Content.ReadAsStringAsync();
                    _logger.LogError("Open-Meteo API returned a non-success status code.");

                    return StatusCode((int)responseMessage.StatusCode, errorBody);
                }

                var responseData = await responseMessage.Content.ReadFromJsonAsync<WeatherApiResponse>();

                if (responseData?.CurrentWeather == null)
                {
                    return NotFound("Weather data not found in successful response.");
                }
                return Ok(responseData.CurrentWeather);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while calling the Open-Meteo API.");
                return StatusCode(500, "An internal error occurred while fetching weather data.");
            }
        }
    }
    public class WeatherApiResponse
    {
        [JsonPropertyName("current_weather")]
        public CurrentWeather? CurrentWeather { get; set; }
    }

    public class CurrentWeather
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("weathercode")]
        public int WeatherCode { get; set; }
    }
}