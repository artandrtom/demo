using System.Collections.Generic;
using System.Linq;
using MEC;

namespace DefaultNamespace
{
    public static class ExtensionMethods
    {
        public static float WaitWhenAll(this IEnumerable<CoroutineHandle> handles)
        {
            return Timing.WaitUntilTrue(() =>
            {
                return handles.All(e => !e.IsRunning);
            });
        }
        
        public static float WaitForCompletion(this CoroutineHandle handle)
        {
            return Timing.WaitUntilTrue(() => !handle.IsRunning);
        }
    }
}