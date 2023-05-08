using System;

namespace DefaultNamespace.AwaitAnything
{
    public class Example
    {
        private async void ExampleAwait()
        {
            await TimeSpan.FromSeconds(2);
        }
        
        private async void Example2Await()
        {
            await 2;
        }
    }
}