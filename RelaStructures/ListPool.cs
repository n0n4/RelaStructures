using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures
{

    // unlike an ArrayPool, a ListPool is capable of expanding its size
    // this makes it easier to use, however it does mean that a ListPool
    // is somewhat less efficient.

    // a key difference here is that upon request, a ListPool never returns
    // null. instead, it will expand its size to supply a new object
    public class ListPool<T> : IPool<T> where T : IPoolable
    {
        private Func<T> CreateFunc; // called to create a new T
        private Action<T> ReturnFunc; // called on an old T when it is returned
        public List<T> Values;
        private List<bool> InUse;
        public int Count { private set; get; }
        private int LastIndex = 0;

        public ListPool(int initialLength, Func<T> createFunc, Action<T> returnFunc)
        {
            CreateFunc = createFunc;
            ReturnFunc = returnFunc;
            Values = new List<T>(initialLength);
            InUse = new List<bool>(initialLength);
            Count = 0;

            for (int i = 0; i < initialLength; i++)
            {
                Values.Add(CreateFunc());
                InUse.Add(false);
            }
        }

        public T Request()
        {
            // check LastIndex first, if that fails, see if any InUse are false, if that fails, expand the list
            if (!InUse[LastIndex])
            {
                InUse[LastIndex] = true;
                Count++;
                int index = LastIndex;
                LastIndex++;
                if (LastIndex >= Values.Count)
                    LastIndex = 0;
                T obj = Values[index];
                obj.SetPoolIndex(index);
                return obj;
            }

            // see if any InUse are false
            int i = LastIndex + 1;
            if (i >= Values.Count)
                i = 0;
            while (i != LastIndex)
            {
                if (!InUse[i])
                {
                    InUse[i] = true;
                    Count++;
                    LastIndex = i + 1;
                    if (LastIndex >= Values.Count)
                        LastIndex = 0;
                    T obj = Values[i];
                    obj.SetPoolIndex(i);
                    return obj;
                }

                i++;
                if (i >= Values.Count)
                    i = 0;
            }

            // no free objects found
            // we are forced to expand the list
            T newobj = CreateFunc();
            Values.Add(newobj);
            InUse.Add(true);
            Count++;
            newobj.SetPoolIndex(Values.Count - 1);
            return newobj;
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

        public void Clear()
        {
            for (int i = 0; i < Values.Count; i++)
            {
                if (InUse[i])
                {
                    InUse[i] = false;
                    ReturnFunc(Values[i]);
                }
            }
            LastIndex = 0;
            Count = 0;
        }
    }
}
