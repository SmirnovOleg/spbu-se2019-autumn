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
                // Prepare data
                Thread.Sleep(SharedData<T>.RandomGenerator.Next(1, Config.MaxSecTimeout) * 1000);
                if (_cancellationRequested)
                {
                    break;
                }
                
                SharedData<T>.ProducingMtx.WaitOne();

                Console.WriteLine($"Producer #{_id} started; buffer size is {SharedData<T>.Buffer.Count}");
                SharedData<T>.Buffer.Add(new T());
                Console.WriteLine($"Producer #{_id} finished writing to buffer");

                SharedData<T>.ProducingMtx.ReleaseMutex();
                SharedData<T>.NonEmptySem.Release();
            }
        }

        public void EndWriting()
        {
            _cancellationRequested = true;
            Console.WriteLine($"Producer #{_id} closed his session");
        }
    }
}