using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UW
{
    public class Playground
    {
        public Playground()
        {
            // test1(null);
        }

        public void test1(object payload)
        {
            // object payload = null;
            var custom = new
            {
                type = "123",
                payload = payload
            };
            var custom_json = "\"custom\" : " + Newtonsoft.Json.JsonConvert.SerializeObject(custom);

            Console.WriteLine(custom_json);

        }
    }
}
