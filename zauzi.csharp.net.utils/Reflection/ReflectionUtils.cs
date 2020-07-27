using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using zauzi.csharp.net.utils.Logging;

namespace zauzi.csharp.net.utils.Reflection
{
    public class ReflectionUtils : IObject
    {
        private static ReflectionUtils instance = new ReflectionUtils();

        #region IObject

        public static IObject Object => (IObject)instance;
        object IObject.GetValue(object obj, string methodName, params Object[] param)
        {
            object result = null;

            try
            {
                MethodInfo method = obj?.GetType()?.GetMethod(methodName);
                result = method?.Invoke(obj, param);
            }catch(Exception ex)
            {
                LogUtils.Default.Write(ex);
            }

            return result;
        }

        #endregion
    }
}
