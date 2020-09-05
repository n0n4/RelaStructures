using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures
{

    public class ListIdPool<T> : IPool<T> where T : IPoolable
    {
        public List<T> Values;
        public List<int> IdsToIndices;
        public List<int> IndicesToIds;
        private Func<T> CreateFunc; // called to create a new T
        private Action<T> ReturnFunc; // called on an old T when it is returned
        public int Count { get; private set; } = 0;

        public ListIdPool(int initialLength, Func<T> createFunc, Action<T> returnFunc)
        {
            CreateFunc = createFunc;
            ReturnFunc = returnFunc;
            Values = new List<T>(initialLength);
            IdsToIndices = new List<int>(initialLength);
            IndicesToIds = new List<int>(initialLength);

            for (int i = 0; i < initialLength; i++)
            {
                Values.Add(CreateFunc());
                IdsToIndices.Add(i);
                IndicesToIds.Add(i);
            }
        }

        public T Request()
        {
            int index = Count;
            Count++;
            if (index >= Values.Count)
            {
                // case: need to expand the list
                T newobj = CreateFunc();
                Values.Add(newobj);
                // for new elements, id always = index
                IdsToIndices.Add(index);
                IndicesToIds.Add(index);
                newobj.SetPoolIndex(index);
                return newobj;
            }

            // case: element already in list
            T obj = Values[index];
            obj.SetPoolIndex(IndicesToIds[index]);
            return obj;
        }

        // reserves a new spot in the list and returns the ID
        // to access, do Values[IdsToIndices[ID]]
        public int RequestId()
        {
            // requesting is simple: due to the return mechanism, the 
            // slot at Count is always the next free spot.

            int index = Count;
            Count++;
            if (index >= Values.Count)
            {
                // case: need to expand the list
                T newobj = CreateFunc();
                Values.Add(newobj);
                // for new elements, id always = index
                IdsToIndices.Add(index);
                IndicesToIds.Add(index);
                newobj.SetPoolIndex(index);
                return index;
            }

            // case: element already in list
            int id = IndicesToIds[index];
            Values[index].SetPoolIndex(id);
            return id;
        }

        public void Return(T obj)
        {
            ReturnId(obj.GetPoolIndex());
        }

        public void ReturnIndex(int index)
        {
            // quick explanation of the return algorithm
            // the hole in the buffer that is formed by
            // returning is patched by moving the last 
            // element in the buffer to that position
            // then swapping the IdsToIndices of those two spots

            // e.g. if we have 
            // Values       A B C D E F
            // IdsToIndices 0 1 2 3 4 5
            // and we remove C, we get:
            // Values       A B F D E _
            // IdsToIndices 0 1 5 3 4 2

            Count--;
            int moveIndex = Count;
            ReturnFunc(Values[index]);
            if (index == moveIndex)
            {
                // trivial case: the last element is the one being removed
                return;
            }

            // swap objects
            T temp = Values[index];
            Values[index] = Values[moveIndex];
            Values[moveIndex] = temp;

            int id = IndicesToIds[index];
            int moveId = IndicesToIds[moveIndex];
            IndicesToIds[index] = moveId;
            IndicesToIds[moveIndex] = id;
            IdsToIndices[id] = moveIndex;
            IdsToIndices[moveId] = index;
        }

        public void ReturnId(int id)
        {
            // this code is duplicated instead of just calling ReturnId(IdsToIndices[id])
            // so that we can avoid looking up id when we already have it on hand.
            int index = IdsToIndices[id];
            Count--;
            int moveIndex = Count;
            ReturnFunc(Values[index]);
            if (index == moveIndex)
            {
                // trivial case: the last element is the one being removed
                return;
            }

            // swap objects
            T temp = Values[index];
            Values[index] = Values[moveIndex];
            Values[moveIndex] = temp;

            int moveId = IndicesToIds[moveIndex];
            IndicesToIds[index] = moveId;
            IndicesToIds[moveIndex] = id;
            IdsToIndices[id] = moveIndex;
            IdsToIndices[moveId] = index;
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++)
            {
                ReturnFunc(Values[i]);
            }
            Count = 0;
        }
    }
}
