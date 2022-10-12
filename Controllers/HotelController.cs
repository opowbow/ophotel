using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using System.Net.Mime;
using System.Threading.Tasks;
using hotels.Domain;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.Linq;

namespace hotels.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class HotelController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _redisEndpointWriteData;
        private List<Hotel> _hotelList = null;

        public HotelController(IConfiguration configuration)
        {
            _configuration = configuration;
            _redisEndpointWriteData = _configuration["_redisEndpointWriteData"];

            _hotelList = new List<Hotel>
            {
                new Hotel() { HotelName = "The Mega Hotel" },
                new Hotel() { HotelName = "The Ok Hotel" },
                new Hotel() { HotelName = "Terrible Hotel" },
                new Hotel() { HotelName = "Jeffs B and B" },
                new Hotel() { HotelName = "Greek Inn" },
                new Hotel() { HotelName = "Barry House" },
                new Hotel() { HotelName = "Zaks Place" }
            };
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try {
                
                Hotel hotel = new Hotel();
                var random = new Random();
                int index = random.Next(_hotelList.Count);
                hotel.HotelName = _hotelList[index].HotelName;
                await Waster();

                bool result = await RedisWrite(hotel);

                return Ok(hotel);
                
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private async Task Waster()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            List<string> xmlValues = new List<string>();
            
            var i=0;
            while(i<=2)
            {
                using (var fileStream = System.IO.File.OpenText("./waster.xml"))
                using(XmlReader reader = XmlReader.Create(fileStream, settings))
                {
                    while(reader.Read())
                    {
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                        xmlValues.Add(reader.Value);
                    }
                }

                i++;
            }    
            
            var foundItems = 0;
            foreach(var item in xmlValues) 
            {   
                if (item.Contains("MyItem10181"))
                {
                    foundItems++;
                }
            }

            foreach(var item in xmlValues) 
            {   
                if (item.Contains("MyItem12181"))
                {
                    foundItems++;
                }
            }

            foreach(var item in xmlValues) 
            {   
                if (item.Contains("MyItem18181"))
                {
                    foundItems++;
                }
            }

            foreach(var item in xmlValues) 
            {   
                if (item.Contains("MyItem2181"))
                {
                    foundItems++;
                }
            }

            foreach(var item in xmlValues) 
            {   
                if (item.Contains("MyItem2281"))
                {
                    foundItems++;
                }
            }
            
            Console.WriteLine("TotalItems:" + xmlValues.Count);
            Console.WriteLine("FoundItems:" + foundItems.ToString());
            await Task.Yield();
        }

        private async Task<bool> RedisWrite(Hotel hotel)
        {
            Console.WriteLine("Writing Data to Redis");
            RedisDataItem redisDataItem = new RedisDataItem();
            redisDataItem.Database = 0;
            redisDataItem.Key = string.Format("{0}{1}","KY_",String.Concat(hotel.HotelName.Where(c => !Char.IsWhiteSpace(c))).ToLower());  
            redisDataItem.Value = hotel.HotelName;
            RedisResponse redisResponse = new RedisResponse();

            try
            {
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using var httpClient = new HttpClient(httpClientHandler);

                    StringContent jsonBody = new StringContent(JsonSerializer.Serialize(redisDataItem), Encoding.UTF8, "application/json");

                    using var response = await httpClient.PostAsync(_redisEndpointWriteData, jsonBody);
                    using var content = response.Content;
                    var result = await content.ReadAsStringAsync();

                    var opts = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    };

                    redisResponse = JsonSerializer.Deserialize<RedisResponse>(result, opts);
                }

                return redisResponse.OperationResult;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred writing Data to Redis");
                throw new Exception("Exception occurred writing Data to Redis", e);
            }
            
        }

    }
}
