using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures.UT
{
    public struct TestStructArr
    {
        public float[] X;
        public float[] Y;
        public int[] Health;
        public void Init()
		{
            X = new float[256];
            Y = new float[256];
            Health = new int[256];
		}
        public void Clear()
        {
            Array.Clear(X, 0, X.Length);
            Array.Clear(Y, 0, Y.Length);
            Array.Clear(Health, 0, Health.Length);
        }

        public void Move(ref TestStructArr b)
        {
            Array.Copy(X, b.X, b.X.Length);
            Array.Copy(Y, b.Y, b.Y.Length);
            Array.Copy(Health, b.Health, b.Health.Length);
        }
    }
}
