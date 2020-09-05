using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures.UT
{
    public struct TestStruct
    {
        public float X;
        public float Y;
        public int Health;

        public void Clear()
        {
            X = 0;
            Y = 0;
            Health = 0;
        }

        public void Move(ref TestStruct b)
        {
            b.X = X;
            b.Y = Y;
            b.Health = Health;
        }
    }
}
