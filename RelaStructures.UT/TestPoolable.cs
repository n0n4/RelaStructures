using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures.UT
{
    public class TestPoolable : IPoolable
    {
        private int PoolIndex = 0;

        public float X = 0;
        public float Y = 0;
        public int Health = 100;

        public void SetPoolIndex(int index)
        {
            PoolIndex = index;
        }
        public int GetPoolIndex()
        {
            return PoolIndex;
        }

        public void Clear()
        {
            X = 0;
            Y = 0;
            Health = 100;
        }
    }
}
