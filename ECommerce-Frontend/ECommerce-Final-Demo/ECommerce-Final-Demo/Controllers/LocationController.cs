using ECommerce_Final_Demo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace ECommerce_Final_Demo.Controllers
{
    public class LocationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl = "https://localhost:7171/api/";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        public LocationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            //_httpClientFactory = httpClientFactory;

        }

        [HttpGet]
        public async Task<JsonResult> LoadCountries()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"{_baseUrl}Cascading/Countrys");

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = JsonSerializer.Deserialize<List<Country>>(await response.Content.ReadAsStringAsync(), JsonOptions);
                return Json(jsonResult);
            }

            return Json(new { error = "Unable to load countries" });
        }


        [HttpGet("States")]
        public async Task<JsonResult> LoadStates([FromQuery] int countryId)
        {

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"{_baseUrl}Cascading/States?countryId={countryId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = JsonSerializer.Deserialize<List<State>>(await response.Content.ReadAsStringAsync(), JsonOptions);

                return Json(jsonResult);
            }

            return Json(new { error = "Unable to load states" });
        }

        [HttpGet("Cities")]
        public async Task<JsonResult> LoadCities([FromQuery] int stateId)
        {


            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"{_baseUrl}Cascading/Cities?stateId={stateId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = JsonSerializer.Deserialize<List<City>>(await response.Content.ReadAsStringAsync(), JsonOptions);

                return Json(jsonResult);
            }

            return Json(new { error = "Unable to load cities" });
        }
    }

}
