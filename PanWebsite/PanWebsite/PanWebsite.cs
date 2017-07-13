using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
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
                Listener.Close();
                Listener.Stop();
                WebsiteThread.Abort();
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

                        // GET Address and Data
                        string addr = context.Request.RawUrl;
                        string address = "";
                        Dictionary<string, string> data = new Dictionary<string, string>();
                        if (addr.Contains("?"))
                        {
                            string[] addr_splitted = addr.Split('?');
                            address = addr_splitted[0];
                            string[] data_str = addr_splitted[1].Split('&');
                            foreach (string kv in data_str)
                            {
                                string[] kv_splitted = kv.Split('=');
                                string key = kv_splitted[0];
                                string val = kv_splitted[1];
                                data.Add(key, val);
                            }
                        }
                        else
                        {
                            address = addr;
                        }

                        // GET Method
                        string method = context.Request.HttpMethod;

                        // GET Cookies
                        Dictionary<string, string> cookies = new Dictionary<string, string>();
                        Cookie[] cookies_original = new Cookie[context.Request.Cookies.Count];
                        context.Request.Cookies.CopyTo(cookies_original, 0);
                        foreach (Cookie c in cookies_original)
                        {
                            cookies.Add(c.Name, c.Value);
                        }

                        PanRequest request = new PanRequest(address, method, data, cookies);
                        PanResponse response = onRequest.Invoke(request);

                        // SET Text
                        buffer = System.Text.Encoding.UTF8.GetBytes(response.ResponseText);

                        // SET Code
                        int code = response.Code;
                        context.Response.StatusCode = code;

                        // SET Cookies
                        context.Response.Cookies = new CookieCollection();
                        string[] c_keys = new string[response.Cookies.Keys.Count]; response.Cookies.Keys.CopyTo(c_keys, 0);
                        string[] c_vals = new string[response.Cookies.Values.Count]; response.Cookies.Values.CopyTo(c_vals, 0);
                        for (int i = 0; i < response.Cookies.Count; i++)
                        {
                            context.Response.Cookies.Add(new Cookie(c_keys[i], c_vals[i]));
                        }

                        //dohere 
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
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
        public readonly string Address;
        public readonly string Method;
        public readonly Dictionary<string, string> Data;
        public readonly Dictionary<string, string> Cookies;
        public PanRequest(string address, string method, Dictionary<string, string> data, Dictionary<string, string> cookies)
        {
            this.Address = address;
            this.Method = method;
            this.Data = data;
            this.Cookies = cookies;
        }
    }
    public class PanResponse
    {
        public string ResponseText;
        public int Code;
        public Dictionary<string, string> Cookies;

        public PanResponse(string responseText, int code, Dictionary<string, string> cookies)
        {
            this.ResponseText = responseText;
            this.Code = code;
            this.Cookies = cookies;
        }
        public PanResponse(string responseText, Dictionary<string, string> cookies)
        {
            this.ResponseText = responseText;
            this.Code = 200;
            this.Cookies = cookies;
        }
        public PanResponse(int code, Dictionary<string, string> cookies)
        {
            this.ResponseText = "";
            this.Code = code;
            this.Cookies = cookies;
        }
    }
}
