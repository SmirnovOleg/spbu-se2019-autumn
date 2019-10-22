using System;
using System.Collections.Generic;

namespace Task03
{
    static class Program
    {
        static void Main()
        {
            List<Consumer<int>> consumers = new List<Consumer<int>>();
            List<Producer<int>> producers = new List<Producer<int>>();
            
            for (int i = 0; i < Config.NumProducers; i++)
            {
                producers.Add(new Producer<int>(i));
            }
            for (int i = 0; i < Config.NumConsumers; i++)
            {
                consumers.Add(new Consumer<int>(i));
            }
            
            while (!Console.KeyAvailable) {}
            
            for (int i = 0; i < Config.NumProducers; i++)
            {
                producers[i].EndWriting();
            }
            for (int i = 0; i < Config.NumConsumers; i++)
            {
                consumers[i].EndReading();
            }
        }
    }
}
