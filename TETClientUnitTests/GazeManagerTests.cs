using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TETCSharpClient;

namespace TETClientUnitTests
{
    [TestClass]
    public class GazeManagerTests
    {
        [TestMethod]
        public void HandleApiResponseTest()
        {
            GazeManager gz = GazeManager.Instance;
            Assert.IsNotNull(gz);
        }
    }
}
