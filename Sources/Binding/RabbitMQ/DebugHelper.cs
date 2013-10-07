using System;
using System.Diagnostics;

namespace MessageBus.Binding.RabbitMQ
{
    internal class DebugHelper
    {
        [ThreadStatic]
        private static Stopwatch _sw;

        public static void Start()
        {
            _sw = Stopwatch.StartNew();
        }

        public static void Stop(string message, params object[] args)
        {
            _sw.Stop();

            object[] copy = new object[args.Length + 1];

            copy[0] = _sw.ElapsedMilliseconds;

            Array.Copy(args, 0, copy, 1, args.Length);

            Debug.Print(message, copy);
        }
    }
}