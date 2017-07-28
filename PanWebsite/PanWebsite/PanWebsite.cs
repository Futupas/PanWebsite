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

                        // GET body
                        Stream inputstream = context.Request.InputStream;

                        // GET Address and Data
                        string[] address = context.Request.Url.AbsolutePath
                            .Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        Dictionary<string, string> data = new Dictionary<string, string>();
                        if(context.Request.Url.Query.Length > 1)
                        {
                            string[] data_str = context.Request.Url.Query.Remove(0, 1)
                                .Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string kv in data_str)
                            {
                                string[] kv_splitted = kv.Split('=');
                                string key = kv_splitted[0];
                                string val = kv_splitted[1];
                                data.Add(key, val);
                            }
                        }

                        // GET Method
                        string method = context.Request.HttpMethod;

                        PanRequest request = new PanRequest(address, method, data, inputstream);
                        PanResponse response = onRequest.Invoke(request);

                        // SET Text
                        buffer = System.Text.Encoding.UTF8.GetBytes(response.ResponseText);

                        // SET Code
                        int code = response.Code;
                        context.Response.StatusCode = code;
                         
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
        public readonly string[] Address;
        public readonly string Method;
        public readonly Dictionary<string, string> Data;
        public readonly Stream InputStream;
        public PanRequest(string[] address, string method, Dictionary<string, string> data, Stream inputStream)
        {
            this.Address = address;
            this.Method = method;
            this.Data = data;
            this.InputStream = inputStream;
        }
        public Dictionary<string, string> PostData()
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
    public class PanResponse
    {
        public string ResponseText;
        public int Code;

        public PanResponse(string responseText, int code)
        {
            this.ResponseText = responseText;
            this.Code = code;
        }
        public PanResponse(string responseText)
        {
            this.ResponseText = responseText;
            this.Code = 200;
        }
        public PanResponse(int code)
        {
            this.ResponseText = "";
            this.Code = code;
        }
    }
}
