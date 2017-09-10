using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using PanWebsite;

namespace PanWebsite
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //PanWebsite website = new PanWebsite("http://localhost:80/", OnRequest);
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
            string text = "hello!";
            int code = 200;

            var cookies = new List<PanCookie>();

            if (request.Address.Length < 1)
            {
                text = "Website MainPage";
            }
            else
            {
                switch (request.Address[0])
                {
                    case "test": text = "test data"; break;
                    case "getdata": text = "GET data"; Console.WriteLine(request.Data["hello"]); break;
                    case "addr4": text = "address 4"; code = 404; break;
                    case "post": text = ""; Console.WriteLine(request.PostData()["suggest"]); break;
                    case "testpost": text = File.ReadAllText(@"D:\PanWebsite\PanWebsite\index.pwhtml"); break;
                    case "cookies":
                        if (request.Address[1] == "get")
                        {
                            Console.WriteLine(request.Cookies.Where(c => c.Name == request.Address[2]).First().Value);
                        }
                        if (request.Address[1] == "set")
                        {
                            if (request.Address[3] == "null")
                                cookies.Add(new PanCookie(request.Address[2], null, "./"));
                            else
                                cookies.Add(new PanCookie(request.Address[2], request.Address[3], "./"));
                        }
                        break;
                    default: text = "else"; break;
                }
            }            

            PanResponse response = new PanResponse(text, code, cookies);
            return response;
        }
    }
}
