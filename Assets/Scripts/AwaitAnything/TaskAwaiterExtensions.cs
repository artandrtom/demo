using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DefaultNamespace.AwaitAnything
{
    public static class TaskAwaiterExtensions
    {
        public static TaskAwaiter GetAwaiter(this TimeSpan timeSpan)
        {
            return Task.Delay(timeSpan).GetAwaiter();
        }
        
        public static TaskAwaiter GetAwaiter(this int integer)
        {
            return Task.Delay(TimeSpan.FromSeconds(integer)).GetAwaiter();
        }
    }
}