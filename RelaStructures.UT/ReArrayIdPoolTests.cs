using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures.UT
{
    [TestClass]
    public class ReArrayIdPoolTests
    {
        [TestMethod]
        public void Req10Test()
        {
            ReArrayIdPool<TestPoolable> pool = new ReArrayIdPool<TestPoolable>(10, 1000,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); });

            List<int> ids = new List<int>();
            for (int i = 0; i < 10; i++)
                ids.Add(pool.RequestId());

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            foreach (int id in ids)
                Assert.AreEqual(id, pool.Values[pool.IdsToIndices[id]].X);
        }

        [TestMethod]
        public void ReqTooManyTest()
        {
            ReArrayIdPool<TestPoolable> pool = new ReArrayIdPool<TestPoolable>(10, 1000,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); });

            List<int> ids = new List<int>();
            for (int i = 0; i < 15; i++)
                ids.Add(pool.RequestId());

            for (int i = 0; i < 15; i++)
            {
                Assert.IsFalse(ids[i] == -1);
            }

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            foreach (int id in ids)
                if (id != -1)
                    Assert.AreEqual(id, pool.Values[pool.IdsToIndices[id]].X);
        }

        [TestMethod]
        public void ReqAndRemoveTest()
        {
            ReArrayIdPool<TestPoolable> pool = new ReArrayIdPool<TestPoolable>(10, 1000,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); });

            List<int> ids = new List<int>();
            for (int i = 0; i < 10; i++)
                ids.Add(pool.RequestId());

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            foreach (int id in ids)
                Assert.AreEqual(id, pool.Values[pool.IdsToIndices[id]].X);

            for (int i = 5; i < 8; i++)
                pool.ReturnId(ids[i]); // return 5, 6, and 7

            Assert.IsTrue(pool.Count == 7); // down from 10
            Assert.IsTrue(pool.Values[pool.IdsToIndices[ids[9]]].X == 9); // verify that accessing id9 still works

            // request 3 new structs, to fill back up to 10
            List<int> newids = new List<int>();
            for (int i = 0; i < 3; i++)
                newids.Add(pool.RequestId());

            foreach (int id in newids)
                Assert.IsFalse(id == -1);

            Assert.IsTrue(pool.Count == 10); // back to 10
            foreach (int id in newids)
                Assert.AreEqual(0, pool.Values[pool.IdsToIndices[id]].X); // verify that returned structs were cleared
        }


        // Ordered tests
        [TestMethod]
        public void OrderedReq10Test()
        {
            ReArrayIdPool<TestPoolable> pool = new ReArrayIdPool<TestPoolable>(10, 1000,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); }, ordered: true);

            List<int> ids = new List<int>();
            for (int i = 0; i < 10; i++)
                ids.Add(pool.RequestId());

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            foreach (int id in ids)
                Assert.AreEqual(id, pool.Values[pool.IdsToIndices[id]].X);
        }

        [TestMethod]
        public void OrderedReqTooManyTest()
        {
            ReArrayIdPool<TestPoolable> pool = new ReArrayIdPool<TestPoolable>(10, 1000,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); }, ordered: true);

            List<int> ids = new List<int>();
            for (int i = 0; i < 15; i++)
                ids.Add(pool.RequestId());

            for (int i = 0; i < 15; i++)
            {
                Assert.IsFalse(ids[i] == -1);
            }

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            foreach (int id in ids)
                if (id != -1)
                    Assert.AreEqual(id, pool.Values[pool.IdsToIndices[id]].X);
        }

        [TestMethod]
        public void OrderedReqAndRemoveTest()
        {
            ReArrayIdPool<TestPoolable> pool = new ReArrayIdPool<TestPoolable>(10, 1000,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); }, ordered: true);

            List<int> ids = new List<int>();
            for (int i = 0; i < 10; i++)
                ids.Add(pool.RequestId());

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            foreach (int id in ids)
                Assert.AreEqual(id, pool.Values[pool.IdsToIndices[id]].X);

            for (int i = 5; i < 8; i++)
                pool.ReturnId(ids[i]); // return 5, 6, and 7

            Assert.IsTrue(pool.Count == 7); // down from 10
            Assert.IsTrue(pool.Values[pool.IdsToIndices[ids[9]]].X == 9); // verify that accessing id9 still works
            Assert.IsTrue(pool.IndicesToIds[pool.Count - 1] == ids[ids.Count - 1]); // verify order was preserved

            // request 3 new structs, to fill back up to 10
            List<int> newids = new List<int>();
            for (int i = 0; i < 3; i++)
                newids.Add(pool.RequestId());

            foreach (int id in newids)
                Assert.IsFalse(id == -1);

            Assert.IsTrue(pool.Count == 10); // back to 10
            foreach (int id in newids)
                Assert.AreEqual(0, pool.Values[pool.IdsToIndices[id]].X); // verify that returned structs were cleared
        }
    }
}
