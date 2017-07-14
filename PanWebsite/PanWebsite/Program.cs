using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace PanWebsite
{
    class Program
    {
        static void Main(string[] args)
        {
            PanWebsite website = new PanWebsite("http://localhost:80/", OnRequest);
            website.Start();
            Console.ReadLine();
            website.Stop();
        }
        static public PanResponse OnRequest(PanRequest request)
        {
            string text = "hello!";
            int code = 200;

            Console.WriteLine(request.Address);
            var cookies = request.Cookies;
            //var cookies = new Dictionary<string, string>();
            switch (request.Address)
            {
                case "/addr1": text = "address 1"; cookies.Add("key1", "value1"); break;
                case "/addr2": text = "address 2"; Console.WriteLine(request.Cookies["key2"]); break;
                case "/addr3": text = "address 3"; cookies["key3"] = "val3"; break;
                case "/addr31": text = "address 31"; cookies["key31"] = "val31"; break;
                case "/addr32": text = "address 32"; cookies["key32"] = "val32"; break;
                case "/addr33": text = "address 33"; cookies["key33"] = "val33"; break;
                case "/addr4": text = "address 4"; code = 404; break;
                default: text = "else"; break;
            }

            PanResponse response = new PanResponse(text, code, cookies);
            return response;
        }
    }
}
