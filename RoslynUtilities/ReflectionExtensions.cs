using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace monacoEditorCSharp.Utilities
{
    public static class ReflectionExtensions
    {
        [DebuggerNonUserCode]
        public static T Invoke<T>(this MethodBase method, object target, params object[] parameters)
        {
            try
            {
                return (T)method.Invoke(target, parameters);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return default(T);
            }
        }
    }
}
