using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PanWebsite
{
    public class PanWebsite
    {
        protected string[] Prefixes;

        protected HttpListener Listener;
        protected Thread WebsiteThread;

        public delegate PanResponse OnRequest(PanRequest request);
        protected OnRequest onRequest;

        public PanWebsite(string[] prefixes, OnRequest request)
        {
            this.Prefixes = prefixes;
            this.onRequest = request;
        }
        public PanWebsite(string prefixe, OnRequest request)
        {
            this.Prefixes = new string[] { prefixe };
            this.onRequest = request;
        }

        public void Start()
        {
            try
            {
                this.Listener = new HttpListener();
                Listener.Prefixes.Clear();
                foreach (string p in this.Prefixes)
                {
                    Listener.Prefixes.Add(p);
                }
                WebsiteThread = new Thread(WebsiteLife);
                Listener.Start();
                WebsiteThread.Start();
            }
            catch (Exception ex)
            {
                throw new WebsiteException(ex);
            }
        }
        public void Stop()
        {
            try
            {
                Listener.Stop();
                Listener.Close();
                WebsiteThread.Abort();
            }
            catch (ThreadAbortException ex)
            {
                //
            }
            catch (Exception ex)
            {
                throw new WebsiteException(ex);
            }
        }

        protected void WebsiteLife()
        {
            try
            {
                while (Listener.IsListening)
                {
                    HttpListenerContext context = Listener.GetContext();
                    Task.Factory.StartNew(() =>
                    {
                        Stream output = context.Response.OutputStream;
                        byte[] buffer;

                        // GET Cookies
                        List<PanCookie> cookies = new List<PanCookie>();
                        foreach (Cookie c in context.Request.Cookies)
                        {
                            cookies.Add(new PanCookie(c.Name, c.Value, c.Path, c.Expires));
                        }

                        // GET Headers
                        Dictionary<string, string[]> headers = new Dictionary<string, string[]>();
                        System.Collections.Specialized.NameValueCollection cheaders = context.Request.Headers;
                        foreach (string key in cheaders.AllKeys)
                        {
                            string current_key = key;
                            string[] currentvalues = cheaders.GetValues(current_key);
                        }

                        // GET Data
                        string url = context.Request.RawUrl; // Url
                        string method = context.Request.HttpMethod; // Method
                        Stream inputStream = context.Request.InputStream; // Body
                        bool hasEntityBody = context.Request.HasEntityBody; // Has Entity Body
                        string[] acceptTypes = context.Request.AcceptTypes; // Accept Types
                        Encoding contentEncoding = context.Request.ContentEncoding; // Content Encoding
                        string contentType = context.Request.ContentType; // Content Type
                        bool isLocal = context.Request.IsLocal; // Is Local
                        string userAgent = context.Request.UserAgent; // User Agent
                        string[] userLanguages = context.Request.UserLanguages; // User Languages

                        PanRequest request = new PanRequest(method, url, inputStream, cookies, hasEntityBody, acceptTypes, contentEncoding, contentType, headers, isLocal, userAgent, userLanguages);
                        PanResponse response = onRequest.Invoke(request);

                        // SET Text
                        buffer = System.Text.Encoding.UTF8.GetBytes(response.ResponseText);

                        // SET Code
                        int code = response.Code;
                        context.Response.StatusCode = code;

                        // SET Cookies
                        foreach (PanCookie c in response.Cookies)
                        {
                            string cookie = "";
                            cookie += (c.Name + "=" + (c.Value == null ? "" : c.Value));
                            if (c.Expires != null)
                            {
                                cookie += ("; Expires=" + c.Expires.ToString());
                            }
                            cookie += ("; Path=" + c.Path);
                            context.Response.Headers.Add("Set-Cookie", cookie);
                        }

                        context.Response.ContentLength64 = buffer.Length;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                        context.Response.Close();
                    });
                }
            }
            catch (ThreadAbortException ex)
            {
                //
            }
            catch (Exception ex)
            {
                throw new WebsiteException(ex);
            }
        }
    }

    public class WebsiteException : Exception
    {
        public Exception InnnerException;
        public WebsiteException(Exception inner_ex)
        {
            this.InnnerException = inner_ex;
        }
    }

    public class PanRequest
    {
        public readonly string Method;
        public readonly string Url; //
        public readonly Stream InputStream;
        public readonly List<PanCookie> Cookies;
        public readonly bool HasEntityBody; //
        public readonly string[] AcceptTypes; //
        public readonly Encoding ContentEncoding; //
        public readonly string ContentType; //
        public readonly Dictionary<string, string[]> Headers; //
        public readonly bool IsLocal; //
        public readonly string UserAgent; //
        public readonly string[] UserLanguages; //

        public PanRequest()
        {
            this.Method = "GET";
            this.Url = "";
            this.InputStream = null;
            this.Cookies = new List<PanCookie>();
            this.HasEntityBody = false;
            this.AcceptTypes = null;
            this.ContentEncoding = Encoding.UTF8;
            this.ContentType = "text/html";
            this.Headers = new Dictionary<string, string[]>();
            this.IsLocal = true;
            this.UserAgent = "";
            this.UserLanguages = null;
        }
        public PanRequest(
            string Method,
            string Url, /**/
            Stream InputStream,
            List<PanCookie> Cookies,
            bool HasEntityBody, /**/
            string[] AcceptTypes, /**/
            Encoding ContentEncoding, /**/
            string ContentType, /**/
            Dictionary<string, string[]> Headers, /**/
            bool IsLocal, /**/
            string UserAgent, /**/
            string[] UserLanguages /**/)
        {
            this.Method = Method;
            this.Url = Url;
            this.InputStream = InputStream;
            this.Cookies = Cookies;
            this.HasEntityBody = HasEntityBody;
            this.AcceptTypes = AcceptTypes;
            this.ContentEncoding = ContentEncoding;
            this.ContentType = ContentType;
            this.Headers = Headers;
            this.IsLocal = IsLocal;
            this.UserAgent = UserAgent;
            this.UserLanguages = UserLanguages;
        }

        public Dictionary<string, string> PostData
        {
            get
            {
                Dictionary<string, string> postdata = new Dictionary<string, string>();
                StreamReader inputstreamreader = new StreamReader(this.InputStream);
                string inputstring = inputstreamreader.ReadToEnd();
                if (inputstring.Length > 0)
                {
                    string[] postdata_str = inputstring.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string kv in postdata_str)
                    {
                        string[] kv_splitted = kv.Split('=');
                        string key = kv_splitted[0];
                        string val = kv_splitted[1];
                        postdata.Add(key, val);
                    }
                }
                return postdata;
            }
        }
        public string[] Address
        {
            get
            {
                string addr_str;
                if (this.Url.Contains("?"))
                {
                    addr_str = this.Url.Split("?".ToCharArray())[0];
                } else
                {
                    addr_str = this.Url;
                }
                string[] addr_arr = addr_str.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                return addr_arr;
            }
        }
        public Dictionary<string, string> Data
        {
            get
            {
                string data_str = "";
                Dictionary<string, string> data = new Dictionary<string, string>();
                if (this.Url.Contains("?"))
                {
                    data_str = this.Url.Split("?".ToCharArray())[0];
                    if (data_str.Length > 0)
                    {
                        string[] data_arr = data_str.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string kv in data_arr)
                        {
                            string[] kv_splitted = kv.Split('=');
                            string key = kv_splitted[0];
                            string val = kv_splitted[1];
                            data.Add(key, val);
                        }
                    }
                }
                return data;
            }
        }
        //public FileStream InputFile { get { } }
    }
    public class PanResponse
    {
        public Stream OutputStream;
        public int Code;
        public List<PanCookie> Cookies;
        public Dictionary<string, string[]> Headers;
        Encoding ContentEncoding;
        public string MIME;

        public PanResponse()
        {
            this.OutputStream = new MemoryStream();
            this.Code = 200;
            this.Cookies = new List<PanCookie>();
            this.Headers = new Dictionary<string, string[]>();
            this.ContentEncoding = Encoding.UTF8;
    }
        public PanResponse(Stream stream, int code, Encoding contentEncoding, List<PanCookie> cookies, Dictionary<string, string[]> headers, string mime)
        {
            this.OutputStream = stream;
            this.Code = code;
            this.Cookies = cookies;
            this.Headers = headers;
            this.ContentEncoding = contentEncoding;
            this.MIME = mime;
        }

        public static PanResponse ReturnContent(string content, Encoding contentEncoding, List<PanCookie> cookies = null) //Return string (content)
        {
            Stream stream = new MemoryStream(contentEncoding.GetBytes(content));
            return new PanResponse(stream, 200, contentEncoding, cookies, null, "text/html");
        }
        public static PanResponse ReturnJson(object o, List<PanCookie> cookies = null) //Return json view of object (as string)
        {
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(o)));
            return new PanResponse(stream, 200, Encoding.UTF8, cookies, null, "application/json");
        }
        public static PanResponse ReturnHtml(string path, Encoding contentEncoding, List<PanCookie> cookies = null) // Return Html page
        {
            string html = File.ReadAllText(path);
            return PanResponse.ReturnContent(html, contentEncoding, cookies);
        }
        public static PanResponse ReturnFile(Stream file, string mime, Encoding contentEncoding, List<PanCookie> cookies = null) //Return File from stream
        {
            //FileStream fileStream = File.Open();
            return new PanResponse(file, 200, contentEncoding, cookies, null, mime);
        }
        public static PanResponse ReturnFile(string path, Encoding contentEncoding, string mime, List<PanCookie> cookies = null) //Return File fron path
        {
            FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
            return new PanResponse(fileStream, 200, contentEncoding, cookies, null, mime);
        }
        public static PanResponse ReturnCode(int code) //Return error
        {
            return new PanResponse(new MemoryStream(), code, Encoding.UTF8, null, null, "");
        }
        public static PanResponse ReturnCode(int code, Encoding contentEncoding, string content) //Return error with page
        {
            Stream stream = new MemoryStream(contentEncoding.GetBytes(content));
            return new PanResponse(stream, code, contentEncoding, null, null, "text/html");
        }
        //public static PanResponse ReturnRedirect(string destination) //Return redirect
        //{
        //    return new PanResponse();
        //}
    }
    public class PanCookie
    {
        public string Name;
        public string Value;
        public string Path;
        public DateTime? Expires;
        public PanCookie(string name, string value, string path = "/", DateTime? expires = null)
        {
            this.Name = name;
            this.Value = value;
            this.Path = path;
            this.Expires = expires;
        }
    }
}
