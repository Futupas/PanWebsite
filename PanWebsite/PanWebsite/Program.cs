using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanWebsite
{
    class Program
    {
        static void Main(string[] args)
        {
            PanWebsite website = new PanWebsite(new string[] { "http://localhost:80/" }, OnRequest);
            website.Start();
            Console.ReadLine();
            website.Stop();
        }
        static public PanResponse OnRequest(PanRequest request)
        {
            string text = "hello!";
            int code = 200;

            Console.WriteLine(request.Address);
            switch (request.Address)
            {
                case "/addr1": text = "address 1"; request.Cookies.Add("key1", "value1"); break;
                case "/addr2": text = "address 2"; Console.WriteLine(request.Cookies["key2"]); break;
                case "/addr3": text = "address 3"; request.Cookies["key3"] = "val3"; break;
                case "/addr4": text = "address 4"; code = 404; break;
                default: text = "else"; break;
            }

            PanResponse response = new PanResponse(text, code, request.Cookies);
            return response;
        }
    }
}
