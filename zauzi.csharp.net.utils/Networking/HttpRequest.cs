using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using zauzi.csharp.net.utils.Logging;
using zauzi.utils.csharp.Networking;

namespace zauzi.csharp.net.utils.Networking
{
    public abstract class HttpRequest
    {
        private Uri Uri = null;

        public HttpRequest(String url)
        {
            this.Uri = new Uri(url);
        }

        public HttpRequest(String protocol, String host, int port)
        {
            UriBuilder uriBuilder = new UriBuilder(protocol, host, port);
            this.Uri = new Uri(uriBuilder?.ToString());
        }

        public interface IResponse
        {
            String message { get; set; }
            Dictionary<String, Object> info { get; set; }
            Object data { get; set; }
            bool? success { get; set; }
        }

        public class HttpClient : System.Net.Http.HttpClient
        {
            System.Net.Http.HttpClient client;
            public Uri Uri { get; private set; }
            public HttpClient(Uri url)
            {
                client = this;
                this.Uri = url;
            }

            private System.Net.Http.HttpClient defaultPerformanceClient()
            {
                var handler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                handler.UseDefaultCredentials = true;
                handler.AllowAutoRedirect = true;
                handler.PreAuthenticate = true;
                handler.UseProxy = false;
                handler.Proxy = null;
                //handler.SupportsProxy = true;
                return new System.Net.Http.HttpClient(handler);
            }



            private System.Net.Http.HttpClient buildClient(string path)
            {
                Uri uri = new Uri(path);
                client = defaultAllowHttpsClient();
                client.DefaultRequestHeaders.ExpectContinue = true;
                client.DefaultRequestHeaders.ConnectionClose = false; // Connection: keep-alive
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client = defaultPerformanceClient();

                client.BaseAddress = this.Uri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return client;
            }

            private System.Net.Http.HttpClient defaultAllowHttpsClient()
            {
                var handler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
                    Proxy = null,
                    MaxConnectionsPerServer = 5000,
                    UseProxy = false,
                    PreAuthenticate = true,
                    AllowAutoRedirect = true,
                    UseDefaultCredentials = true,
                    UseCookies = true,
                };
                handler.PreAuthenticate = true;

                return new System.Net.Http.HttpClient(handler);
            }

            public new Task<HttpResponseMessage> GetAsync(string path, HttpCompletionOption httpCompletionOption)
            {
                client = buildClient(path);
                return client.GetAsync(path, httpCompletionOption);
            }

            public Task<HttpResponseMessage> PostAsJsonAsync(string path, object value)
            {
                client = buildClient(path);
                return client.PostAsJsonAsync(path, value);
            }

        }

        private string parseString(int cCount, string target)
        {
            bool over = target?.Count() > cCount;
            return over ? string.Format("{0}...", new object[] { target.Substring(0, cCount) }) : target;
        }

        #region "GET METHOD"                
        protected async Task<IResponse> getDataByGet<T>(String path, Dictionary<String, String> parameter)
        {
            using (var client = new HttpClient(this.Uri))
            {
                try
                {
                    IResponse resultList = null;

                    client.BaseAddress = client.Uri;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    path = processPath(path, parameter);

                    HttpContent content = new StringContent("", Encoding.UTF8, "application/json");
                    LogUtils.Default.Write(string.Format("getDataByGet.begin"));
                    HttpResponseMessage response = await client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                    String s = response?.Content?.ReadAsStringAsync()?.Result;
                    LogUtils.Default.Write(string.Format("Response (status: {0}, url:{1})", new object[] { response?.IsSuccessStatusCode ?? false, path }));
                    LogUtils.Default.Write(string.Format("({0})", new object[] { parseString(100, s) }));
                    LogUtils.Default.Write(string.Format("getDataByGet.response"));
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            LogUtils.Default.Write("Response parsing...");
                            string json = response.Content.ReadAsStringAsync().Result;
                            //json = Properties.Resources.Test;
                            resultList = JsonUtils.DeserializeObject<IResponse>(json);
                            LogUtils.Default.Write("Response parsing success.");
                        }
                        catch (Exception e)
                        {
                            LogUtils.Default.Write(e);
                        }
                    }
                    return resultList;
                }
                catch (Exception e)
                {
                    LogUtils.Default.Write(e);
                    throw;
                }
            }
        }

        protected   async Task<List<T>> getListDataByGet<T>(String path, Dictionary<String, String> parameter)
        {
            using (var client = new HttpClient(this.Uri))
            {
                try
                {
                    List<T> resultList = new List<T>();

                    client.BaseAddress = client.Uri;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    path = processPath(path, parameter);

                    HttpContent content = new StringContent("", Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        String s = response.Content.ReadAsStringAsync().Result;
                        try
                        {
                            resultList = JsonUtils.DeserializeObject<List<T>>(response.Content.ReadAsStringAsync().Result);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    return resultList;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        protected   T getDataByGetSync<T>(String path, Dictionary<String, String> parameter)
        {
            return getDataByGet<T>(path, parameter).GetAwaiter().GetResult();
        }

        protected   List<T> getListDataByGetSync<T>(String path, Dictionary<String, String> parameter)
        {
            return getListDataByGet<T>(path, parameter).GetAwaiter().GetResult();
        }

        private   String processPath(String path, Dictionary<String, String> parameter)
        {
            path = String.Format(path);
            for (int a = 0; a < (parameter?.Count ?? 0); a++)
            {
                if ((parameter.Values?.ElementAt(a)?.Contains("'") ?? false)) throw new Exception("Parameter can't have (') symbol");
                path = path.Replace(parameter.Keys.ElementAt(a), parameter.Values.ElementAt(a));
            }
            return path;
        }

        #endregion

        #region "POST METHOD"
        protected   async Task<T> getDataByPost<T>(String path, Object parameter)
        {
            return await getDataByPost<T>(path, parameter, false);
        }
        protected   async Task<T> getDataByPost<T>(String path, Object parameter, Boolean useView)
        {
            using (var client = new HttpClient(this.Uri))
            {
                try
                {
                    T result = default(T);

                    client.BaseAddress = client.Uri;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //var json = JsonConvert.SerializeObject(parameter);
                    BaseRequest brMap = new BaseRequest();
                    brMap.parameter = parameter;

                    //HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsJsonAsync(path, useView ? parameter : brMap).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            String json = await response.Content.ReadAsStringAsync();
                            if (typeof(T) == typeof(String))
                            {
                                Object s = json;
                                return (T)s;
                            }
                            result = JsonUtils.DeserializeObject<T>(json);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        protected   async Task<T> getDataByPost<T>(String path, Dictionary<string, string> parameters)
        {
            using (var client = new HttpClient(this.Uri))
            {
                try
                {
                    T result = default(T);

                    client.BaseAddress = client.Uri;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    FormUrlEncodedContent encodedContent = new FormUrlEncodedContent(parameters);

                    //HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(path, encodedContent).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            if (typeof(T) == typeof(String))
                            {
                                Object s = response.Content.ReadAsStringAsync().Result;
                                return (T)s;
                            }
                            result = JsonUtils.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        protected async Task<List<T>> getListDataByPost<T>(String path, Dictionary<string, string> parameters)
        {
            using (var client = new HttpClient(this.Uri))
            {
                try
                {
                    List<T> result = default(List<T>);

                    client.BaseAddress = client.Uri;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    FormUrlEncodedContent encodedContent = new FormUrlEncodedContent(parameters);

                    //HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(path, encodedContent).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        String s = response.Content.ReadAsStringAsync().Result;
                        try
                        {
                            result = JsonUtils.DeserializeObject<List<T>>(response.Content.ReadAsStringAsync().Result);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        protected async Task<List<T>> getListDataByPost<T>(String path, Object parameter, Boolean useView)
        {
            using (var client = new HttpClient(this.Uri))
            {
                try
                {
                    List<T> resultList = new List<T>();

                    client.BaseAddress = client.Uri;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//"application/x-www-form-urlencoded"));//

                    BaseRequest brMap = new BaseRequest();
                    brMap.parameter = parameter;

                    HttpResponseMessage response = await client.PostAsJsonAsync(path, useView ? parameter : brMap).ConfigureAwait(false);// content).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        String s = response.Content.ReadAsStringAsync().Result;
                        try
                        {
                            resultList = JsonUtils.DeserializeObject<List<T>>(response.Content.ReadAsStringAsync().Result);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    return resultList;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        protected   async Task<List<T>> getListDataByPost<T, K>(String path, Object parameter)
        {
            return await getListDataByPost<T>(path, parameter, false);
        }

        protected   T getDataByPostSync<T>(String path, Object parameter, Boolean useView)
        {
            return getDataByPost<T>(path, parameter, useView).GetAwaiter().GetResult();
        }

        protected   List<T> getListDataByPostSync<T>(String path, Object parameter, Boolean useView)
        {
            return getListDataByPost<T>(path, parameter, useView).GetAwaiter().GetResult();
        }

        protected   T getDataByPostSync<T>(String path, Object parameter)
        {
            return getDataByPost<T>(path, parameter, false).GetAwaiter().GetResult();
        }

        protected   List<T> getListDataByPostSync<T, K>(String path, Object parameter)
        {
            return getListDataByPost<T>(path, parameter, false).GetAwaiter().GetResult();
        }

        protected   T getDataByPostFormParamSync<T>(String path, Dictionary<string, string> parameters)
        {
            return getDataByPost<T>(path, parameters).GetAwaiter().GetResult();
        }

        protected   List<T> getListDataByPostFormParamSync<T>(String path, Dictionary<string, string> parameters)
        {
            return getListDataByPost<T>(path, parameters).GetAwaiter().GetResult();
        }
        #endregion

        class BaseRequest
        {
            public Object parameter { get; set; }
        }
    }
}
