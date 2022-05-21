using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures
{
    public class StructReArray<T> where T : struct
    {
        public delegate void ClearDelegate(ref T obj);
        public delegate void MoveDelegate(ref T from, ref T to);
        public delegate void InitDelegate(ref T obj);

        public T[] Values;
        public int[] IdsToIndices;
        public int[] IndicesToIds;
        /// <summary>
        /// <para>Called when we clear out a struct</para>
        /// <para>Client should revert target index back to default values</para>
        /// <para>e.g. Values[index].X = 0</para>
        /// </summary>
        public ClearDelegate ClearAction;
        /// <summary>
        /// <para>Called when we move a struct, args are "values, moving index, target index"</para>
        /// <para>What is required here is that the client provide the logic for copying one struct into another</para>
        /// <para>e.g. Values[targetIndex].X = Values[movingIndex].X</para>
        /// </summary>
        public MoveDelegate MoveAction;
        /// <summary>
        /// <para>called when we construct or resize a struct</para>
        /// <para>alllows the client to preallocate arrays inside the struct</para>
        /// <para>e.g. Values[targetIndex].X = new int[256]</para>
        /// </summary>
        public InitDelegate InitAction;
        public int Count { get; private set; } = 0;
        public int Length { get; private set; } = 0;
        public int MaxLength;

        public StructReArray(int length, int maxlength, ClearDelegate clearAction, MoveDelegate moveAction, InitDelegate initAction = null)
        {
            ClearAction = clearAction;
            MoveAction = moveAction;
            InitAction = initAction;

            Length = length;
            MaxLength = maxlength;
            Values = new T[length];
            IdsToIndices = new int[length];
            IndicesToIds = new int[length];

            for (int i = 0; i < Length; i++)
            {
                Values[i] = new T();
                IdsToIndices[i] = i;
                IndicesToIds[i] = i;
                InitAction?.Invoke(ref Values[i]);
            }
        }

        // reserves a new spot in the list and returns the ID
        // to access, do Values[IdsToIndices[ID]]
        public int Request()
        {
            // requesting is simple: due to the return mechanism, the 
            // slot at Count is always the next free spot.

            int index = Count;
            if (index >= Length)
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
            Values = nv;
            IdsToIndices = nidtoi;
            IndicesToIds = nitoid;
            Length = newsize;
            // fill in future ids
            for (int i = Count; i < Length; i++)
            {
                Values[i] = new T();
                IdsToIndices[i] = i;
                IndicesToIds[i] = i;
                InitAction?.Invoke(ref Values[i]);
            }
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
            if (index == moveIndex)
            {
                // trivial case: the last element is the one being removed
                ClearAction(ref Values[index]);
                return;
            }

            MoveAction(ref Values[moveIndex], ref Values[index]);
            ClearAction(ref Values[moveIndex]);

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
            if (index == moveIndex)
            {
                // trivial case: the last element is the one being removed
                ClearAction(ref Values[index]);
                return;
            }

            MoveAction(ref Values[moveIndex], ref Values[index]);
            ClearAction(ref Values[moveIndex]);

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
                ClearAction(ref Values[i]);
            }
            Count = 0;
        }

        public void AddRange(StructReArray<T> range)
        {
            for (int i = 0; i < range.Count; i++)
            {
                int newid = Request();
                if (newid == -1)
                    return; // hit our limit
                int newindex = IdsToIndices[newid];
                Values[newindex] = range.Values[i];
            }
        }
        public ref T this[int i]
        {
            get { return ref Values[i]; }
        }
        public ref T AtId(int id)
        {
            return ref Values[IdsToIndices[id]];
        }
    }
}
