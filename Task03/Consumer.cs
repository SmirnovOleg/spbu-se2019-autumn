using System;
using System.Linq;
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
                SharedData<T>.NonEmptySem.Wait();
                
                if (_cancellationRequested)
                {
                    break;
                }
                
                SharedData<T>.ConsumingMtx.WaitOne();
                
                Console.WriteLine($"Consumer #{_id} started; buffer size is {SharedData<T>.Buffer.Count}");
                T unused = SharedData<T>.Buffer.First();
                SharedData<T>.Buffer.RemoveAt(0);
                Console.WriteLine($"Consumer #{_id} finished extracting item from buffer");
                
                SharedData<T>.ConsumingMtx.ReleaseMutex();
                
                Thread.Sleep(SharedData<T>.RandomGenerator.Next(1, Config.MaxSecTimeout) * 1000);
            }
        }

        public void EndReading()
        {
            _cancellationRequested = true;
            Console.WriteLine($"Consumer #{_id} closed his session");
        }
    }
}