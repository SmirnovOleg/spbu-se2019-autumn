using System;
using System.Threading;

namespace Task03
{
    public class Consumer<T>
    {
        private readonly int _id;
        private bool _cancellationRequested;

        public Consumer(int id)
        {
            _id = id;
            var thread = new Thread(Read);
            Console.WriteLine($"Consumer #{_id} opened his session");
            thread.Start();
        }

        private void Read()
        {
            while (!_cancellationRequested)
            {
                // Wait for reading, increment active readers amount
                // Capture mutex for writing if only previous reader haven't do it yet
                SharedData<T>.WaitingForWorkMtx.WaitOne();
                SharedData<T>.ReadingHelperMtx.WaitOne();
                SharedData<T>.NowReading++;
                if (SharedData<T>.NowReading == 1)
                {
                    SharedData<T>.WritingSem.Wait();
                }

                SharedData<T>.ReadingHelperMtx.ReleaseMutex();
                SharedData<T>.WaitingForWorkMtx.ReleaseMutex();

                // Reading
                int bufferSize = SharedData<T>.Buffer.Count;
                if (bufferSize == 0)
                {
                    Console.WriteLine($"Consumer #{_id} couldn't start reading because buffer is empty");
                    Thread.Sleep(SharedData<T>.RandomGenerator.Next(1, Config.MaxSecTimeout) * 1000);
                }
                else
                {
                    int index = SharedData<T>.RandomGenerator.Next(0, bufferSize);
                    Console.WriteLine($"Consumer #{_id} started reading from buffer[{index}]");
                    T unused = SharedData<T>.Buffer[index];
                    Thread.Sleep(SharedData<T>.RandomGenerator.Next(1, Config.MaxSecTimeout) * 1000);
                    Console.WriteLine($"Consumer #{_id} finished reading from buffer[{index}]");
                }

                // Decrement active readers amount, release mutex for writing
                SharedData<T>.ReadingHelperMtx.WaitOne();
                SharedData<T>.NowReading--;
                if (SharedData<T>.NowReading == 0)
                {
                    SharedData<T>.WritingSem.Release();
                }
                SharedData<T>.ReadingHelperMtx.ReleaseMutex();

                if (_cancellationRequested)
                {
                    Console.WriteLine($"Consumer #{_id} closed his session");
                }
            }
        }

        public void EndReading()
        {
            _cancellationRequested = true;
        }
    }
}