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
                //PanWebsite website = new PanWebsite("http://localhost:82/", OnRequest);
                PanWebsite website = new PanWebsite("http://192.168.0.111:80/", OnRequest);
                website.Start();
                Console.ReadLine();
                website.Stop();
                Console.ReadLine();
            }
            catch (Exception ex)
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

            Console.WriteLine(request.Url + " " + request.IsLocal.ToString() + " " + request.UserIP);
            if (request.Address.Length < 1)
            {
                return PanResponse.ReturnContent("Emtry page", Encoding.UTF8);
            }
            else
            {
                switch (request.Address[0])
                {
                    case "favicon.ico": return PanResponse.ReturnFile(@"E:\PROJECTS\PanWebsite\Website2\favicon.ico", Encoding.UTF8); break;
                    case "content": return PanResponse.ReturnContent("Content", Encoding.UTF8); break;
                    case "html": return PanResponse.ReturnHtml(@"E:\PROJECTS\PanWebsite\Website2\index.html", Encoding.UTF8); break;
                    case "code": return PanResponse.ReturnCode(500); break;
                    case "file": return PanResponse.ReturnFile(@"E:\PROJECTS\PanWebsite\Website2\image.jpg", Encoding.UTF8); break;
                    case "json": return PanResponse.ReturnJson(new { a = 5, s = "fff", b = true }); break;
                    case "upload":
                        if (request.Address.Length == 1)
                        {
                            return PanResponse.ReturnHtml(@"E:\PROJECTS\PanWebsite\Website2\upload.html", Encoding.UTF8); break;
                        }
                        else if (request.Address[1] == "api")
                        {
                            if (request.HasEntityBody)
                            {
                                var d = request.MutlipartFormData;
                                foreach (var item in d)
                                {
                                    if (item.Filename != "")
                                    {
                                        FileStream fs = File.Open(Path.Combine(@"E:\PROJECTS\PanWebsite\Website2\downloads", item.Filename), FileMode.Create, FileAccess.ReadWrite);
                                        item.Data.Position = 0;
                                        fs.Position = 0;
                                        item.Data.CopyTo(fs);
                                        fs.Close();
                                        fs.Dispose();
                                    }
                                }
                                return PanResponse.ReturnCode(200);
                            }
                            else
                            {
                                return PanResponse.ReturnCode(500);
                            }
                        }
                        else
                        {
                            return PanResponse.ReturnCode(500); break;
                        }
                        break;
                    default: return PanResponse.ReturnCode(404); break;
                }
            }
        }
    }
}
