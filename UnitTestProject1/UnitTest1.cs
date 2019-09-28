using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Caesar_s_code.LettersSupportProvider.TypeLettersSupport;
using static Caesar_s_code.LettersSupportProvider;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [DataTestMethod]
        [DataRow(russian)]
        public void MyTest(TypeLettersSupport sup)
        {
            
        }
    }
}
