using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace PanWebsite
{
    class Program
    {
        static void Main(string[] args)
        {
            //PanWebsite website = new PanWebsite("http://localhost:80/", OnRequest);
            PanWebsite website = new PanWebsite("http://192.168.0.111:80/", OnRequest);
            website.Start();
            Console.ReadLine();
            website.Stop();
            Console.ReadLine();
        }
        static public PanResponse OnRequest(PanRequest request)
        {
            string text = "hello!";
            int code = 200;

            //Console.WriteLine("Segms: ", request.Address.Length);
            switch (request.Address[0])
            {
                case "addr1": text = "address 1"; break;
                case "addr2": text = "address 2"; break;
                case "addr3": text = "address 3"; break;
                case "addr31": text = "address 31"; Console.WriteLine(request.Data["hello"]); break;
                case "addr32": text = "address 32"; break;
                case "addr33": text = "address 33"; break;
                case "addr4": text = "address 4"; code = 404; break;
                case "post": text = ""; Console.WriteLine(request.PostData()["suggest"]); break;
                case "testpost": text = File.ReadAllText(@"D:\PanWebsite\PanWebsite\index.pwhtml"); break;
                default: text = "else"; break;
            }

            PanResponse response = new PanResponse(text, code);
            return response;
        }
    }
}
