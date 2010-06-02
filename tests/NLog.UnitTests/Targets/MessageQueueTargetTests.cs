using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NLog.Targets;
using NLog.Config;

namespace NLog.UnitTests.Targets
{
    /// <summary>
    /// Summary description for MessageQueueTargetTests
    /// </summary>
    [TestClass]
    public class MessageQueueTargetTests : NLogTestBase
    {
       
        private Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.MessageQueueTargetTests");

       

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethod1()
        {
            MessageQueueTarget mqt = new MessageQueueTarget();
            mqt.Queue = ".\\private$\\nlog";
            mqt.Layout = "${level} ${message}";
            SimpleConfigurator.ConfigureForTargetLogging(mqt, LogLevel.Trace);
            
            logger.Debug("aaa");
            logger.Info("bbb");
            logger.Warn("ccc");
        }
    }
}
