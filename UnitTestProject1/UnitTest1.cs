using Microsoft.VisualStudio.TestTools.UnitTesting;
using static MyNamespace.MyEnum;
using MyNamespace;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [DataTestMethod]
        [DataRow(first)]
        public void MyTest1(MyEnum a) { }

        [DataTestMethod]
        [DataRow(0)]
        public void MyTest2(int a) { }
    }
}
