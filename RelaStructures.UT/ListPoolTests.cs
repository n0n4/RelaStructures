using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures.UT
{
    [TestClass]
    public class ListPoolTests
    {
        [TestMethod]
        public void Req10Test()
        {
            ListPool<TestPoolable> pool = new ListPool<TestPoolable>(10,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); });

            List<TestPoolable> objs = new List<TestPoolable>();
            for (int i = 0; i < 10; i++)
                objs.Add(pool.Request());

            foreach (var o in objs)
                Assert.IsFalse(o == null);

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, objs[i].X);
        }

        [TestMethod]
        public void ReqTooManyTest()
        {
            ListPool<TestPoolable> pool = new ListPool<TestPoolable>(10,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); });

            List<TestPoolable> objs = new List<TestPoolable>();
            for (int i = 0; i < 15; i++)
                objs.Add(pool.Request());

            for (int i = 0; i < 15; i++)
            {
                Assert.IsFalse(objs[i] == null);
            }

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            for (int i = 0; i < 15; i++)
                Assert.AreEqual(i, objs[i].X);
        }

        [TestMethod]
        public void ReqAndRemoveTest()
        {
            ListPool<TestPoolable> pool = new ListPool<TestPoolable>(10,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); });

            List<TestPoolable> objs = new List<TestPoolable>();
            for (int i = 0; i < 10; i++)
                objs.Add(pool.Request());

            foreach (var o in objs)
                Assert.IsFalse(o == null);

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, objs[i].X);

            for (int i = 5; i < 8; i++)
                pool.Return(objs[i]); // return 5, 6, and 7

            Assert.IsTrue(pool.Count == 7); // down from 10
            Assert.IsTrue(objs[9].X == 9); // verify that accessing id9 still works

            // request 3 new structs, to fill back up to 10
            List<TestPoolable> newobjs = new List<TestPoolable>();
            for (int i = 0; i < 3; i++)
                newobjs.Add(pool.Request());

            foreach (var o in newobjs)
                Assert.IsFalse(o == null);

            Assert.IsTrue(pool.Count == 10); // back to 10
            foreach (var o in newobjs)
                Assert.AreEqual(0, o.X); // verify that returned structs were cleared
        }
    }
}
