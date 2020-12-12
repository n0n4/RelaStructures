using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures.UT
{
    public struct TestStructAdv
    {
        public float Timer;
        public Action Callback;

        public void Setup(float timer, Action callback)
        {
            Timer = timer;
            Callback = callback;
        }

        public void Clear()
        {
            Timer = 0;
            Callback = null;
        }

        public void Move(ref TestStructAdv b)
        {
            b.Timer = Timer;
            b.Callback = Callback;
        }
    }
}
