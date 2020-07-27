using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zauzi.chsarp.net.utils.Models;

namespace zauzi.chsarp.net.utils.Networking
{
    public class ResponseStatus : Response
    {
        public T getData<T>(Options options = null)
        {
            T result = default(T); 

            try
            {
                String json = JsonConvert.SerializeObject(this.data);
                result = JsonConvert.DeserializeObject<T>(json);
            }
            catch(Exception ex)
            {
                if (options?.Exception ?? false) throw;
            }
            

            return result;
        }

        #region support
        public sealed class Options
        {
            public bool Exception { get; set; } = false;
        }
        #endregion
    }
}
