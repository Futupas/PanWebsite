using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
//using l = PanWebsiteLocal;
using PanWebsite;

namespace PanWebsite
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //PanWebsite website = new PanWebsite("http://localhost:82/", OnRequest);
                PanWebsite website = new PanWebsite("http://192.168.0.111:80/", OnRequest);
                website.Start();
                Console.ReadLine();
                website.Stop();
                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception!");
                Console.WriteLine(ex.InnerException.Message);
                Console.ReadLine();
            }
            
        }

        static public PanResponse OnRequest(PanRequest request)
        {
            var cookies = new List<PanCookie>();
            
            Console.WriteLine(request.Url);
            //return PanResponse.ReturnContent("Emtry page", Encoding.UTF8);
            if (request.Address.Length < 1)
            {
                return PanResponse.ReturnContent("Emtry page", Encoding.UTF8);
            }
            else
            {
                switch (request.Address[0])
                {
                    case "content": return PanResponse.ReturnContent("Content", Encoding.UTF8); break;
                    case "html": return PanResponse.ReturnHtml(@"E:\PROJECTS\PanWebsite\Website\index.html", Encoding.UTF8); break;
                    case "code": return PanResponse.ReturnCode(500); break;
                    case "file": return PanResponse.ReturnFile(@"E:\PROJECTS\PanWebsite\Website\image.jpg", Encoding.UTF8); break;
                    default: return PanResponse.ReturnCode(404); break;
                }
            }
        }
    }
}
