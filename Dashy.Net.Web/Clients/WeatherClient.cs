using System.Globalization;
using System.Net.Http;

public class WeatherClient(HttpClient httpClient, ILogger<WeatherClient> logger)
{

    public async Task<CurrentWeather?> GetWeatherAsync(double lat, double lon, string unit)
    {
        try
        {
            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lonStr = lon.ToString(CultureInfo.InvariantCulture);
            var url = $"/api/weather?latitude={latStr}&longitude={lonStr}&unit={unit}";

            return await httpClient.GetFromJsonAsync<CurrentWeather>(url);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
public record CurrentWeather(double Temperature, int WeatherCode);