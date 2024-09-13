using ECommerce_Final_Demo.FileUpload;
using ECommerce_Final_Demo.Models;
using System.Net.Http;
using System.Text.Json;

namespace ECommerce_Final_Demo.Helper
{

    public class StoreLocation
    {
       
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly string _baseUrl = "https://localhost:7171/api/";

            public StoreLocation(IHttpClientFactory httpClientFactory)
            {
                _httpClientFactory = httpClientFactory;
            }

        public async Task<string> GetCountryNameAsync(int countryId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{_baseUrl}Cascading/countries/{countryId}";

                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var country = JsonSerializer.Deserialize<Country>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Adjust as necessary
                    });

                    return country?.Name ?? "Unknown";
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);

            }
            return "Unknown";
        }

            public async Task<string> GetStateNameAsync(int stateId)
            {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{_baseUrl}Cascading/states/{stateId}";

                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var State = JsonSerializer.Deserialize<State>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Adjust as necessary
                    });

                    return State?.Name ?? "Unknown";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            // Consider logging the error or throwing an exception
                return "Unknown";
            }

            public async Task<string> GetCityNameAsync(int cityId)
            {
           
                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{_baseUrl}Cascading/cities/{cityId}";

                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var City = JsonSerializer.Deserialize<City>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Adjust as necessary
                    });

                    return City?.Name ?? "Unknown";
                }
            

            // Consider logging the error or throwing an exception
            return "Unknown";
            }

        
    }
   }

