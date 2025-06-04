# PooledListManagerProject


    using System;
    using System.Collections.Generic;
    
    public class TestClass
    {
        private readonly PooledListManager<int> _intListPool = new(32, 128);
    
        public void Run()
        {
            using var pooled = _intListPool.Rent(out var list);
    
            list.Add(10);
            list.Add(20);
            list.Add(30);
    
            foreach (var item in list)
            {
                Console.WriteLine($"Item: {item}");
            }
        }
    }
