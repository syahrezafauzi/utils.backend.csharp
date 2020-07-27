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
        T IObject.GetValue<T>(object obj, string methodName, params Object[] param)
        {
            T result = default(T);

            try
            {
                MethodInfo method = obj?.GetType()?.GetMethod(methodName);
                result = (T) method?.Invoke(obj, param);
            }catch(Exception ex)
            {
                LogUtils.Default.Write(ex);
            }

            return result;
        }

        #endregion
    }
}
