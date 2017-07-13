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

            Console.WriteLine(request.Address);

            PanResponse response = new PanResponse(text, 200, request.Cookies);
            return response;
        }
    }
}
