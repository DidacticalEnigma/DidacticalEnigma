using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace JDict.Tests
{
    public class ExpectactionTester<T>
    {
        private List<Func<T, object>> expectactions = new List<Func<T, object>>();
        private T expected;

        public ExpectactionTester(T expected)
        {
            this.expected = expected;
        }

        public ExpectactionTester<T> Expect(Func<T, object> operation)
        {
            expectactions.Add(operation);
            return this;
        }

        public ExpectactionTester<T> Expect(Action<T> operation)
        {
            expectactions.Add(e =>
            {
                operation(e);
                return null;
            });
            return this;
        }

        public void Test(T actual)
        {
            foreach (var expectaction in expectactions)
            {
                object returnValActual = null;
                object returnValExpected = null;
                Exception exceptionActual = null;
                Exception exceptionExpected = null;
                try
                {
                    returnValActual = expectaction(actual);
                }
                catch (Exception ex)
                {
                    exceptionActual = ex;
                }

                try
                {
                    returnValExpected = expectaction(expected);
                }
                catch (Exception ex)
                {
                    exceptionExpected = ex;
                }
                Assert.AreEqual(exceptionExpected, exceptionActual);
                Assert.AreEqual(returnValExpected, returnValActual);
            }
            
        }
    }
}
