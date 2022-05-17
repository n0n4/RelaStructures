using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace RelaStructures.UT
{
    [TestClass]
    public class StructReArrayTests
    {
        public void ClearAction(ref TestStruct obj)
        {
            obj.Clear();
        }

        public void MoveAction(ref TestStruct from, ref TestStruct to)
        {
            from.Move(ref to);
        }

        public void ClearActionAdv(ref TestStructAdv obj)
        {
            obj.Clear();
        }

        public void MoveActionAdv(ref TestStructAdv from, ref TestStructAdv to)
        {
            from.Move(ref to);
        }

        [TestMethod]
        public void Req10Test()
        {
            StructReArray<TestStruct> pool = new StructReArray<TestStruct>(10, 1000,
                ClearAction, MoveAction);

            List<int> ids = new List<int>();
            for (int i = 0; i < 10; i++)
                ids.Add(pool.Request());

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            for (int i = 0; i < pool.Count; i++)
                pool[i].X += i;

            foreach (int id in ids)
                Assert.AreEqual(id, pool.AtId(id).X);
        }

        [TestMethod]
        public void ReqTooManyTest()
        {
            StructReArray<TestStruct> pool = new StructReArray<TestStruct>(10, 1000,
                ClearAction, MoveAction);

            List<int> ids = new List<int>();
            for (int i = 0; i < 15; i++)
                ids.Add(pool.Request());

            for (int i = 0; i < 15; i++)
            {
                Assert.IsFalse(ids[i] == -1);
            }

            for (int i = 0; i < pool.Count; i++)
                pool[i].X += i;

            foreach (int id in ids)
                if (id != -1)
                    Assert.AreEqual(id, pool.AtId(id).X);

            Assert.AreEqual(pool.Count, 15);
        }

        [TestMethod]
        public void ReqAndRemoveTest()
        {
            StructReArray<TestStruct> pool = new StructReArray<TestStruct>(10, 1000,
                ClearAction, MoveAction);

            List<int> ids = new List<int>();
            for (int i = 0; i < 10; i++)
                ids.Add(pool.Request());

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            for (int i = 0; i < pool.Count; i++)
                pool[i].X += i;

            foreach (int id in ids)
                Assert.AreEqual(id, pool.AtId(id).X);

            for (int i = 5; i < 8; i++)
                pool.ReturnId(ids[i]); // return 5, 6, and 7

            Assert.IsTrue(pool.Count == 7); // down from 10
            Assert.IsTrue(pool.AtId(ids[9]).X == 9); // verify that accessing id9 still works

            // request 3 new structs, to fill back up to 10
            List<int> newids = new List<int>();
            for (int i = 0; i < 3; i++)
                newids.Add(pool.Request());

            foreach (int id in newids)
                Assert.IsFalse(id == -1);

            Assert.IsTrue(pool.Count == 10); // back to 10
            foreach (int id in newids)
                Assert.AreEqual(0, pool.AtId(id).X); // verify that returned structs were cleared
        }

        [TestMethod]
        public void ReqAdv10Test()
        {
            StructReArray<TestStructAdv> pool = new StructReArray<TestStructAdv>(10, 1000,
                ClearActionAdv, MoveActionAdv);

            int testpasses = 0;
            Action testCallback = () => { testpasses++; };

            List<int> ids = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                int id = pool.Request();
                ids.Add(id);
                pool.AtId(id).Setup(i, testCallback);
            }

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            foreach (int id in ids)
            {
                pool.AtId(id).Callback();
            }

            Assert.AreEqual(ids.Count, testpasses);
        }

        [TestMethod]
        public void ReqAdvTooManyTest()
        {
            StructReArray<TestStructAdv> pool = new StructReArray<TestStructAdv>(10, 1000,
                ClearActionAdv, MoveActionAdv);

            int testpasses = 0;
            Action testCallback = () => { testpasses++; };

            List<int> ids = new List<int>();
            for (int i = 0; i < 15; i++)
            {
                int id = pool.Request();
                ids.Add(id);
                pool.AtId(id).Setup(i, testCallback);
            }

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            foreach (int id in ids)
            {
                pool.AtId(id).Callback();
            }

            Assert.AreEqual(ids.Count, testpasses);
        }

        private static void ClearActionFloat(ref FloatStruct obj)
        {
            obj.F = 0;
        }

        private static void MoveActionFloat(ref FloatStruct from, ref FloatStruct to)
        {
            to.F = from.F;
        }

        [TestMethod]
        public void SRAOverflow1()
        {
            float expected = 1;
            RelaStructures.StructReArray<FloatStruct> Sample = new RelaStructures.StructReArray<FloatStruct>(10, 20, ClearActionFloat, MoveActionFloat);
            for (int i = 0; i < Sample.MaxLength; i++)
            {
                int index = Sample.Request();
                Sample.AtId(index) = new FloatStruct(f: expected);
            }
            Assert.AreEqual(expected, Sample[0].F);
        }

        [TestMethod]
        public void SRAOverflow2()
        {
            float expected = 1;
            RelaStructures.StructReArray<FloatStruct> Sample = new RelaStructures.StructReArray<FloatStruct>(10, 20, ClearActionFloat, MoveActionFloat);
            for (int i = 0; i < Sample.MaxLength; i++)
            {
                int index = Sample.Request();
                int sampleIndex = Sample.IdsToIndices[index];
                Sample[sampleIndex] = new FloatStruct(f: expected);
            }
            Assert.AreEqual(expected, Sample[10].F);
        }
    }
}
