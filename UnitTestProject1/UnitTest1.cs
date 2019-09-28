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

        [DataTestMethod]
        [DataRow(MyNamespace2.MyEnum2.C)]
        public void MyTest3(MyNamespace2.MyEnum2 a) { }
    }
}

namespace MyNamespace2
{
    public enum MyEnum2
    {
        C = 0
    }
}
