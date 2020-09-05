using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures
{

    public class ArrayPool<T> : IPool<T> where T : IPoolable
    {
        private Func<T> CreateFunc; // called to create a new T
        private Action<T> ReturnFunc; // called on an old T when it is returned
        private int Length;
        public int Count { private set; get; }
        public T[] Values;
        private bool[] InUse;
        private int LastIndex = 0;
        public ArrayPool(int length, Func<T> createFunc, Action<T> returnFunc)
        {
            CreateFunc = createFunc;
            ReturnFunc = returnFunc;
            Length = length;
            Count = 0;
            Values = new T[Length];
            InUse = new bool[Length];

            // create the initial objects in the pool
            for (int i = 0; i<Length; i++)
            {
                Values[i] = CreateFunc();
            }
        }

        public T Request()
        {
            // check LastIndex first, if that fails, see if any InUse are false, if that fails, return null
            if (!InUse[LastIndex])
            {
                InUse[LastIndex] = true;
                int index = LastIndex;
                LastIndex++;
                if (LastIndex >= Length)
                    LastIndex = 0;
                T obj = Values[index];
                obj.SetPoolIndex(index);
                Count++;
                return obj;
            }

            // see if any InUse are false
            int i = LastIndex + 1;
            if (i >= Length)
                i = 0;
            while (i != LastIndex)
            {
                if (!InUse[i])
                {
                    InUse[i] = true;
                    LastIndex = i + 1;
                    if (LastIndex >= Length)
                        LastIndex = 0;
                    T obj = Values[i];
                    obj.SetPoolIndex(i);
                    Count++;
                    return obj;
                }

                i++;
                if (i >= Length)
                    i = 0;
            }

            return default(T); // no free objects found
        }

        public void Return(T obj)
        {
            // when an object is returned, we need to determine which index it is
            // and free that index

            // do any cleanup on the object
            ReturnFunc(obj);

            int index = obj.GetPoolIndex();
            InUse[index] = false;
            Count--;
        }

        public bool IsFull()
        {
            return Count == Length;
        }

        public void Clear()
        {
            for (int i = 0; i < Length; i++)
            {
                if (InUse[i])
                {
                    InUse[i] = false;
                    ReturnFunc(Values[i]);
                }
            }
            LastIndex = 0;
        }
        }
}
