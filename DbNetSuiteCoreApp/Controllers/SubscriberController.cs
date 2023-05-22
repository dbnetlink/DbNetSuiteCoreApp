using DbNetSuiteCoreApp.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DbNetSuiteCoreApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriberController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SubscriberController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://api.sender.net");
            string accessToken = configuration.GetValue<string>("SenderAPI:AccessToken");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _configuration = configuration;
        }

        // GET: api/<SubscriberController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<SubscriberController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Subscriber subscriber)
        {
            string emailAddress = subscriber.EmailAddress;
            string betaProgrammeGroup = _configuration.GetValue<string>("SenderAPI:BetaProgrammeGroup");

            try
            {
                SenderSubscriber senderSubscriber = new SenderSubscriber() { Email = emailAddress, Groups = new string[] { betaProgrammeGroup } };

                var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                string json = JsonSerializer.Serialize(senderSubscriber, options);
                var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/v2/subscribers", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    return new JsonResult(subscriber);
                }
                else
                {
                    // Handle the error response
                    var senderResponse = JsonSerializer.Deserialize<SenderResponse>(await response.Content.ReadAsStringAsync());

                    subscriber.Error = true;
                    subscriber.Message = senderResponse.Message;

                    return new JsonResult(subscriber);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // PUT api/<SubscriberController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SubscriberController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}