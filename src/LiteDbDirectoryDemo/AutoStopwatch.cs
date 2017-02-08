using System;
using System.Diagnostics;

namespace SQLiteDirectoryDemo
{
    /// <summary>
    /// <see cref="Stopwatch"/> helper class to show the start and stop time on Console
    /// </summary>
    public class AutoStopWatch : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _message;
        public AutoStopWatch(string message)
        {
            _message = message;
            Console.WriteLine("{0} starting ", message);
            _stopwatch = Stopwatch.StartNew();
        }


        #region IDisposable Members
        public void Dispose()
        {

            _stopwatch.Stop();
            long ms = _stopwatch.ElapsedMilliseconds;

            Console.WriteLine("{0} Finished {1} ms", _message, ms);
        }
        #endregion
    }
}
