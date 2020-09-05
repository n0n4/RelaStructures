using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures
{

    public class ReArrayIdPool<T> : IPool<T> where T : IPoolable
    {
        public T[] Values;
        public int[] IdsToIndices;
        public int[] IndicesToIds;
        private Func<T> CreateFunc; // called to create a new T
        private Action<T> ReturnFunc; // called on an old T when it is returned
        public int Count { get; private set; } = 0;
        public int Length { get; private set; } = 0;
        public int MaxLength;
        public bool Ordered = false; // if true: order is preserved when an item is returned

        public ReArrayIdPool(int length, int maxlength, Func<T> createFunc, Action<T> returnFunc,
            bool ordered = false)
        {
            CreateFunc = createFunc;
            ReturnFunc = returnFunc;
            Length = length;
            MaxLength = maxlength;
            Values = new T[length];
            IdsToIndices = new int[length];
            IndicesToIds = new int[length];
            Ordered = ordered;

            for (int i = 0; i < length; i++)
            {
                Values[i] = CreateFunc();
                IdsToIndices[i] = i;
                IndicesToIds[i] = i;
            }
        }

        public T Request()
        {
            int index = Count;
            if (index >= Values.Length)
            {
                // case: out of indices
                if (Length >= MaxLength)
                    return default(T); // we cannot offer a new struct

                // resize the array
                int newsize = Length * 2;
                if (newsize >= MaxLength)
                    newsize = MaxLength;
                Resize(newsize);
            }
            Count++;

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
            if (index >= Values.Length)
            {
                // case: out of indices
                if (Length >= MaxLength)
                    return -1; // -1 indicates that we cannot offer a new struct

                // resize the array
                int newsize = Length * 2;
                if (newsize >= MaxLength)
                    newsize = MaxLength;
                Resize(newsize);
            }
            Count++;

            // case: element already in list
            int id = IndicesToIds[index];
            Values[index].SetPoolIndex(id);
            return id;
        }

        public void Resize(int newsize)
        {
            T[] nv = new T[newsize];
            int[] nidtoi = new int[newsize];
            int[] nitoid = new int[newsize];

            for (int i = 0; i < Count; i++)
            {
                nv[i] = Values[i];
                nidtoi[i] = IdsToIndices[i];
                nitoid[i] = IndicesToIds[i];
            }
            for (int i = Count; i < newsize; i++)
            {
                nv[i] = CreateFunc();
                nidtoi[i] = i;
                nitoid[i] = i;
            }
            
            Values = nv;
            IdsToIndices = nidtoi;
            IndicesToIds = nitoid;
            Length = newsize;
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

            if (Ordered)
            {
                // move every higher indexed object one down
                T temp = Values[index];
                int lastid = IndicesToIds[index];
                for (int i = index; i < Count; i++)
                {
                    Values[i] = Values[i + 1];
                    int id = IndicesToIds[i + 1];
                    IndicesToIds[i] = id;
                    IdsToIndices[id] = i;
                }
                Values[Count] = temp;
                IndicesToIds[Count] = lastid;
                IdsToIndices[lastid] = Count;
                return;
            }
            else
            {
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

            if (Ordered)
            {
                // move every higher indexed object one down
                T temp = Values[index];
                for (int i = index; i < Count; i++)
                {
                    Values[i] = Values[i + 1];
                    int mid = IndicesToIds[i + 1];
                    IndicesToIds[i] = mid;
                    IdsToIndices[mid] = i;
                }
                Values[Count] = temp;
                IndicesToIds[Count] = id;
                IdsToIndices[id] = Count;
                return;
            }
            else
            {
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
