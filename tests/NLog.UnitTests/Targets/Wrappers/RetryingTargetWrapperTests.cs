// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

namespace NLog.UnitTests.Targets.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    [TestClass]
    public class RetryingTargetWrapperTests : NLogTestBase
	{
        [TestMethod]
        public void RetryingTargetWrapperTest1()
        {
            var target = new MyTarget();
            var wrapper = new RetryingTargetWrapper()
            {
                WrappedTarget = target,
                RetryCount = 10,
                RetryDelayMilliseconds = 1,
            };

            ((ISupportsInitialize)wrapper).Initialize();
            ((ISupportsInitialize)target).Initialize();

            var events = new LogEventInfo[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello"),
            };

            var exceptions = new List<Exception>();
            
            var continuations = new AsyncContinuation[events.Length];
            for (int i = 0; i < continuations.Length; ++i)
            {
                continuations[i] = exceptions.Add;
            }

            wrapper.WriteLogEvents(events, continuations);

            // make sure all events went through
            Assert.AreEqual(3, target.Events.Count);
            Assert.AreSame(events[0], target.Events[0]);
            Assert.AreSame(events[1], target.Events[1]);
            Assert.AreSame(events[2], target.Events[2]);

            Assert.AreEqual(continuations.Length, exceptions.Count, "Some continuations were not invoked.");

            // make sure there were no exception
            foreach (var ex in exceptions)
            {
                Assert.IsNull(ex);
            }
        }

        [TestMethod]
        public void RetryingTargetWrapperTest2()
        {
            var target = new MyTarget()
            {
                ThrowExceptions = 6,
            };

            var wrapper = new RetryingTargetWrapper()
            {
                WrappedTarget = target,
                RetryCount = 4,
                RetryDelayMilliseconds = 1,
            };

            ((ISupportsInitialize)wrapper).Initialize();
            ((ISupportsInitialize)target).Initialize();

            var events = new LogEventInfo[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello"),
            };

            var exceptions = new List<Exception>();

            var continuations = new AsyncContinuation[events.Length];
            for (int i = 0; i < continuations.Length; ++i)
            {
                continuations[i] = exceptions.Add;
            }

            var internalLogOutput = RunAndCaptureInternalLog(() => wrapper.WriteLogEvents(events, continuations), LogLevel.Trace);
            string expectedLogOutput = @"Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 1/4
Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 2/4
Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 3/4
Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 4/4
Warn Too many retries. Aborting.
Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 1/4
Warn Error while writing to 'MyTarget': System.InvalidOperationException: Some exception has ocurred.. Try 2/4
";
            Assert.AreEqual(expectedLogOutput, internalLogOutput);

            // first event does not get to wrapped target because of too many attempts.
            // second event gets there in 3rd retry
            // and third event gets there immediately
            Assert.AreEqual(2, target.Events.Count);
            Assert.AreSame(events[1], target.Events[0]);
            Assert.AreSame(events[2], target.Events[1]);

            Assert.AreEqual(continuations.Length, exceptions.Count, "Some continuations were not invoked.");

            Assert.IsNotNull(exceptions[0]);
            Assert.AreEqual("Some exception has ocurred.", exceptions[0].Message);
            Assert.IsNull(exceptions[1]);
            Assert.IsNull(exceptions[2]);
        }

        public class MyTarget : Target
        {
            public MyTarget()
            {
                this.Events = new List<LogEventInfo>();
            }

            public List<LogEventInfo> Events { get; set; }

            public int ThrowExceptions { get; set; }

            protected override void Write(LogEventInfo logEvent, AsyncContinuation asyncContinuation)
            {
                if (this.ThrowExceptions-- > 0)
                {
                    asyncContinuation(new InvalidOperationException("Some exception has ocurred."));
                    return;
                }

                this.Events.Add(logEvent);
                asyncContinuation(null);
            }

            protected override void Write(LogEventInfo logEvent)
            {
            }
        }
    }
}
