using System;
using System.Threading;

namespace Task03
{
    public class Producer<T> where T : new()
    {
        private readonly int _id;
        private bool _cancellationRequested;

        public Producer(int id)
        {
            _id = id;
            var thread = new Thread(Write);
            Console.WriteLine($"Producer #{_id} opened his session");
            thread.Start();
        }

        private void Write()
        {
            while (!_cancellationRequested)
            {
                SharedData<T>.WaitingForWorkMtx.WaitOne();
                SharedData<T>.WritingSem.Wait();

                Console.WriteLine($"Producer #{_id} started writing to buffer");
                SharedData<T>.Buffer.Add(new T());
                Thread.Sleep(SharedData<T>.RandomGenerator.Next(1, Config.MaxSecTimeout) * 1000);
                Console.WriteLine($"Producer #{_id} finished writing to buffer");

                SharedData<T>.WritingSem.Release();
                SharedData<T>.WaitingForWorkMtx.ReleaseMutex();
                
                if (_cancellationRequested)
                {
                    Console.WriteLine($"Producer #{_id} closed his session");
                }
            }
        }

        public void EndWriting()
        {
            _cancellationRequested = true;
        }
    }
}