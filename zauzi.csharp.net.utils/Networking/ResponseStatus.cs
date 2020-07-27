using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zauzi.chsarp.net.utils.Models;
using zauzi.csharp.net.utils.Logging;

namespace zauzi.chsarp.net.utils.Networking
{
    public class ResponseStatus : Response
    {
        public T getData<T>(Options options = null)
        {
            T result = default(T);
            if (data == null) return result;
            try
            {
                string target = data.ToString();
                result = JsonConvert.DeserializeObject<T>(target);
            }
            catch (Exception ex)
            {
                LogUtils.Default.Write(ex);
            }

            return result;
        }

        public String getMessage(MessageStyle style = MessageStyle.Default)
        {
            String result = null;
            List<String> holder;

            try
            {
                switch (style)
                {
                    case MessageStyle.Default:
                        holder = new List<string>();
                        holder.Add(this?.message);
                        var data = this?.getData<object[]>();
                        List<String> message = data?.Cast<JObject>().Select(x => Convert.ToString(x?.GetValue("message")))?.ToList();
                        if ((message?.Count ?? 0) > 0) holder.AddRange(message);
                        result = String.Join("\n", holder);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogUtils.Default.Write(ex);
            }

            return result;
        }

        #region support
        public sealed class Options
        {
            public bool Exception { get; set; } = false;
        }

        public enum MessageStyle
        {
            Default
        }
        #endregion
    }
}
