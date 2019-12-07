using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Task05;

namespace Tests.FineGrained
{
    [TestFixture]
    public class ParallelScenarios
    {
        private readonly Random _random = new Random();
        
        [TestCase(30, 1000, 1000, 10)]
        public void InsertFindConcurrently_ManyValuesToFineTree_UpdateTree(int workers, int maxSize, 
            int maxValue, int maxTimeout)
        {
            var expectedTree = new FineGrainedSyncBinaryTree<int>();
            var actualTree = new FineGrainedSyncBinaryTree<int>();
            var elementsToInsert = new ConcurrentQueue<int>();
            for (int i = 0; i < maxSize; ++i)
            {
                var value = _random.Next(maxValue);
                elementsToInsert.Enqueue(value);
                expectedTree.Insert(value);
            }
            var elementsToFind = new ConcurrentQueue<int>(elementsToInsert);
            Task[] tasks = new Task[workers];   
            
            for (int i = 0; i < workers; ++i)
            {
                if (_random.Next(2) == 0)
                {
                    // Create worker to test insertion
                    tasks[i] = Task.Run(() =>
                    {
                        while (!elementsToInsert.IsEmpty)
                        {
                            if (elementsToInsert.TryDequeue(out var value))
                            {
                                actualTree.Insert(value);
                            }
                            Thread.Sleep(_random.Next(maxTimeout));
                        }
                    });
                }
                else
                {
                    // Create worker to test search
                    tasks[i] = Task.Run(() =>
                    {
                        while (!elementsToFind.IsEmpty)
                        {
                            if (elementsToFind.TryDequeue(out var value))
                            {
                                actualTree.Find(value);
                            }
                            Thread.Sleep(_random.Next(maxTimeout));
                        }
                    });
                }
            }
            
            Task.WaitAll(tasks);
            Assert.IsTrue(Utils.ContentEquals(actualTree, expectedTree));
            Assert.IsTrue(Utils.ValidateStructure(actualTree));
        }
        
        [TestCase(30, 1000, 1000, 10)]
        public void InsertSequentially_RemoveFindConcurrently_ManyValuesToFineTree_UpdateTree(int workers, 
            int maxSize, int maxValue, int maxTimeout)
        {
            var expectedTree = new FineGrainedSyncBinaryTree<int>();
            var actualTree = new FineGrainedSyncBinaryTree<int>();
            var elementsToInsert = new ConcurrentQueue<int>();
            var elementsToRemove = new ConcurrentQueue<int>();
            for (int i = 0; i < maxSize; ++i)
            {
                var value = _random.Next(maxValue);
                elementsToInsert.Enqueue(value);
                actualTree.Insert(value);
                expectedTree.Insert(value);
                value = _random.Next(maxValue);
                elementsToRemove.Enqueue(value);
            }
            var elementsToFind = new ConcurrentQueue<int>(elementsToInsert);
            foreach (int value in elementsToRemove)
            {
                expectedTree.Remove(value);
            }
            Task[] tasks = new Task[workers];
            
            for (int i = 0; i < workers; ++i)
            {
                if (_random.Next(2) == 0)
                {
                    // Create worker to test removing
                    tasks[i] = Task.Run(() =>
                    {
                        while (!elementsToRemove.IsEmpty)
                        {
                            if (elementsToRemove.TryDequeue(out var value))
                            {
                                actualTree.Remove(value);
                            }
                            Thread.Sleep(_random.Next(maxTimeout));
                        }
                    });
                }
                else
                {
                    // Create worker to test search
                    tasks[i] = Task.Run(() =>
                    {
                        while (!elementsToFind.IsEmpty)
                        {
                            if (elementsToFind.TryDequeue(out var value))
                            {
                                actualTree.Find(value);
                            }
                            Thread.Sleep(_random.Next(maxTimeout));
                        }
                    });
                }
            }
            
            Task.WaitAll(tasks);
            Assert.IsTrue(Utils.ContentEquals(actualTree, expectedTree));
            Assert.IsTrue(Utils.ValidateStructure(actualTree));
        }
        
        [TestCase(100, 10000, 10000, 10)]
        public void InsertRemoveFindConcurrently_ManyValuesToFineTree_UpdateTree(int workers, 
            int maxSize, int maxValue, int maxTimeout)
        {
            var actualTree = new FineGrainedSyncBinaryTree<int>();
            var elementsToInsert = new ConcurrentQueue<int>();
            var elementsToRemove = new ConcurrentQueue<int>();
            for (int i = 0; i < maxSize; ++i)
            {
                var value = _random.Next(maxValue);
                elementsToInsert.Enqueue(value);
                value = _random.Next(maxValue);
                elementsToRemove.Enqueue(value);
            }
            var elementsToFind = new ConcurrentQueue<int>(elementsToInsert);
            Task[] tasks = new Task[workers];
            
            for (int i = 0; i < workers; ++i)
            {
                int actionId = _random.Next(3);
                if (actionId == 0)
                {
                    // Create worker to test insertion
                    tasks[i] = Task.Run(() =>
                    {
                        while (!elementsToInsert.IsEmpty)
                        {
                            if (elementsToInsert.TryDequeue(out var value))
                            {
                                actualTree.Insert(value);
                            }
                            Thread.Sleep(_random.Next(maxTimeout));
                        }
                    });
                }
                else if (actionId == 1)
                {
                    // Create worker to test removing
                    tasks[i] = Task.Run(() =>
                    {
                        while (!elementsToRemove.IsEmpty)
                        {
                            if (elementsToRemove.TryDequeue(out var value))
                            {
                                actualTree.Remove(value);
                            }
                            Thread.Sleep(_random.Next(maxTimeout));
                        }
                    });
                }
                else 
                {
                    // Create worker to test search
                    tasks[i] = Task.Run(() =>
                    {
                        while (!elementsToFind.IsEmpty)
                        {
                            if (elementsToFind.TryDequeue(out var value))
                            {
                                actualTree.Find(value);
                            }
                            Thread.Sleep(_random.Next(maxTimeout));
                        }
                    });
                }
            }
            
            Task.WaitAll(tasks);
            Assert.IsTrue(Utils.ValidateStructure(actualTree));
        }
        
        [TestCase(30, 1000, 10000, 10)]
        public void InsertSequentially_InsertFindConcurrently_ManyValuesToFineTree_UpdateTree(int workers, 
            int maxSize, int maxValue, int maxTimeout)
        {
            var actualTree = new FineGrainedSyncBinaryTree<int>();
            var elementsToInsertSeq = new ConcurrentQueue<int>();
            var elementsToInsertConcur = new ConcurrentQueue<int>();
            for (int i = 0; i < maxSize; ++i)
            {
                var value = _random.Next(maxValue);
                elementsToInsertSeq.Enqueue(value);
                actualTree.Insert(value);
                value = _random.Next(maxValue);
                elementsToInsertConcur.Enqueue(value);
            }
            var elementsToFind = new ConcurrentQueue<int>(elementsToInsertSeq);
            Task[] tasks = new Task[workers];
            
            for (int i = 0; i < workers; ++i)
            {
                if (_random.Next(2) == 0)
                {
                    // Create worker to test insertion
                    tasks[i] = Task.Run(() =>
                    {
                        while (!elementsToInsertConcur.IsEmpty)
                        {
                            if (elementsToInsertConcur.TryDequeue(out var value))
                            {
                                actualTree.Insert(value);
                            }
                            Thread.Sleep(_random.Next(maxTimeout));
                        }
                    });
                }
                else
                {
                    // Create worker to test search
                    tasks[i] = Task.Run(() =>
                    {
                        while (!elementsToFind.IsEmpty)
                        {
                            if (elementsToFind.TryDequeue(out var value))
                            {
                                Assert.AreEqual(value, actualTree.Find(value));
                            }
                            Thread.Sleep(_random.Next(maxTimeout));
                        }
                    });
                }
            }
            
            Task.WaitAll(tasks);
            Assert.IsTrue(Utils.ValidateStructure(actualTree));
        }
    }
}