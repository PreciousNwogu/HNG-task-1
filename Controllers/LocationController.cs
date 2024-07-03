using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace LocationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string visitor_name)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (clientIp == "::1")
            {
                 clientIp = "102.189.0.242";
                //clientIp = "127.0.0.1";
                //clientIp = "10.0.0.79";
            }

            // Get location data
            var locationClient = new RestClient($"http://ip-api.com/json/{clientIp}");
            var locationRequest = new RestRequest();
            var locationResponse = await locationClient.GetAsync(locationRequest);
            var locationContent = locationResponse.Content;

            // Ensure the response content is not null
            if (locationContent == null)
            {
                return StatusCode(500, "Unable to fetch location data.");
            }

            var locationData = JObject.Parse(locationContent);
            var city = locationData["city"]?.ToString() ?? "Unknown";

            // Get weather data
            var weatherClient = new RestClient($"http://api.openweathermap.org/data/2.5/weather?q={city}&appid=682127720f607ae460d7efe471282cfc&units=metric");
            var weatherRequest = new RestRequest();
            var weatherResponse = await weatherClient.GetAsync(weatherRequest);
            var weatherContent = weatherResponse.Content;

            // Ensure the response content is not null
            if (weatherContent == null)
            {
                return StatusCode(500, "Unable to fetch weather data.");
            }

            var weatherData = JObject.Parse(weatherContent);
            var temperature = weatherData["main"]?["temp"]?.ToString() ?? "N/A";

            var greeting = $"Hello, {visitor_name}!, the temperature is {temperature} degrees Celsius in {city}";

            var response = new
            {
                client_ip = clientIp,
                location = city,
                greeting = greeting
            };

            return Ok(response);
        }
    }
}