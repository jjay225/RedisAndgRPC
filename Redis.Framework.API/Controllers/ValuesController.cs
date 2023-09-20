using Redis.ServiceBridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Redis.Framework.API.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            var randomKey = new Random().Next(1, 4);
            var redisResults = RedisBridge.GetRecord<IEnumerable<string>>($"FrameWorkKey:{randomKey}");
            var resultsLocal = new string[] { "value1", "value2" };
            if (redisResults != null)
            {
                return redisResults;
            }

            RedisBridge.SetRecord($"FrameWorkKey:{randomKey}", redisResults, TimeSpan.FromMinutes(5));

            return resultsLocal;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
