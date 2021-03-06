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
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Internal;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using System.Collections.Generic;

    [TestClass]
    public class AsyncTargetWrapperTests : NLogTestBase
	{
        [TestMethod]
        public void AsyncTargetWrapperInitTest()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new AsyncTargetWrapper(myTarget, 300, AsyncTargetWrapperOverflowAction.Grow);
            Assert.AreEqual(AsyncTargetWrapperOverflowAction.Grow, targetWrapper.OverflowAction);
            Assert.AreEqual(300, targetWrapper.QueueLimit);
            Assert.AreEqual(50, targetWrapper.TimeToSleepBetweenBatches);
            Assert.AreEqual(100, targetWrapper.BatchSize);
        }

        [TestMethod]
        public void AsyncTargetWrapperInitTest2()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new AsyncTargetWrapper()
            {
                WrappedTarget = myTarget,
            };

            Assert.AreEqual(AsyncTargetWrapperOverflowAction.Discard, targetWrapper.OverflowAction);
            Assert.AreEqual(10000, targetWrapper.QueueLimit);
            Assert.AreEqual(50, targetWrapper.TimeToSleepBetweenBatches);
            Assert.AreEqual(100, targetWrapper.BatchSize);
        }

        [TestMethod]
        public void AsyncTargetWrapperSyncTest1()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new AsyncTargetWrapper
            {
                WrappedTarget = myTarget,
            };
            ((ISupportsInitialize)targetWrapper).Initialize();
            ((ISupportsInitialize)myTarget).Initialize();

            var logEvent = new LogEventInfo();
            Exception lastException = null;
            ManualResetEvent continuationHit = new ManualResetEvent(false);
            Thread continuationThread = null;
            AsyncContinuation continuation =
                ex =>
                    {
                        lastException = ex;
                        continuationThread = Thread.CurrentThread;
                        continuationHit.Set();
                    };

            targetWrapper.WriteLogEvent(logEvent, continuation);

            // continuation was not hit 
            continuationHit.WaitOne();
            Assert.AreNotSame(continuationThread, Thread.CurrentThread);
            Assert.IsNull(lastException);
            Assert.AreEqual(1, myTarget.WriteCount);

            continuationHit.Reset();
            targetWrapper.WriteLogEvent(logEvent, continuation);
            continuationHit.WaitOne();
            Assert.AreNotSame(continuationThread, Thread.CurrentThread);
            Assert.IsNull(lastException);
            Assert.AreEqual(2, myTarget.WriteCount);
        }

        [TestMethod]
        public void AsyncTargetWrapperAsyncTest1()
        {
            var myTarget = new MyAsyncTarget();
            var targetWrapper = new AsyncTargetWrapper(myTarget);
            ((ISupportsInitialize)targetWrapper).Initialize();
            ((ISupportsInitialize)myTarget).Initialize();
            var logEvent = new LogEventInfo();
            Exception lastException = null;
            var continuationHit = new ManualResetEvent(false);
            AsyncContinuation continuation =
                ex =>
                {
                    lastException = ex;
                    continuationHit.Set();
                };

            targetWrapper.WriteLogEvent(logEvent, continuation);

            continuationHit.WaitOne();
            Assert.IsNull(lastException);
            Assert.AreEqual(1, myTarget.WriteCount);

            continuationHit.Reset();
            targetWrapper.WriteLogEvent(logEvent, continuation);
            continuationHit.WaitOne();
            Assert.IsNull(lastException);
            Assert.AreEqual(2, myTarget.WriteCount);
        }

        [TestMethod]
        public void AsyncTargetWrapperAsyncWithExceptionTest1()
        {
            var myTarget = new MyAsyncTarget
            {
                ThrowExceptions = true,
            };

            var targetWrapper = new AsyncTargetWrapper(myTarget);
            ((ISupportsInitialize)targetWrapper).Initialize();
            ((ISupportsInitialize)myTarget).Initialize();
            var logEvent = new LogEventInfo();
            Exception lastException = null;
            var continuationHit = new ManualResetEvent(false);
            AsyncContinuation continuation =
                ex =>
                {
                    lastException = ex;
                    continuationHit.Set();
                };

            targetWrapper.WriteLogEvent(logEvent, continuation);

            continuationHit.WaitOne();
            Assert.IsNotNull(lastException);
            Assert.IsInstanceOfType(lastException, typeof(InvalidOperationException));

            // no flush on exception
            Assert.AreEqual(0, myTarget.FlushCount);
            Assert.AreEqual(1, myTarget.WriteCount);

            continuationHit.Reset();
            lastException = null;
            targetWrapper.WriteLogEvent(logEvent, continuation);
            continuationHit.WaitOne();
            Assert.IsNotNull(lastException);
            Assert.IsInstanceOfType(lastException, typeof(InvalidOperationException));
            Assert.AreEqual(0, myTarget.FlushCount);
            Assert.AreEqual(2, myTarget.WriteCount);
        }

        [TestMethod]
        public void AsyncTargetWrapperFlushTest()
        {
            var myTarget = new MyAsyncTarget
            {
                ThrowExceptions = true,
               
            };

            var targetWrapper = new AsyncTargetWrapper(myTarget)
            {
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
            };

            ((ISupportsInitialize)targetWrapper).Initialize();
            ((ISupportsInitialize)myTarget).Initialize();

            List<Exception> exceptions = new List<Exception>();

            int eventCount = 5000;

            for (int i = 0; i < eventCount; ++i)
            {
                targetWrapper.WriteLogEvent(LogEventInfo.CreateNullEvent(),
                    ex =>
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    });
            }

            Exception lastException = null;
            ManualResetEvent mre = new ManualResetEvent(false);

            string internalLog = RunAndCaptureInternalLog(
                () =>
                {
                    targetWrapper.Flush(
                        cont =>
                        {
                            try
                            {
                                // by this time all continuations should be completed
                                Assert.AreEqual(eventCount, exceptions.Count);

                                // with just 1 flush of the target
                                Assert.AreEqual(1, myTarget.FlushCount);

                                // and all writes should be accounted for
                                Assert.AreEqual(eventCount, myTarget.WriteCount);
                            }
                            catch (Exception ex)
                            {
                                lastException = ex;
                            }
                            finally
                            {
                                mre.Set();
                            }
                        });
                    mre.WaitOne();
                },
                LogLevel.Trace);

            if (lastException != null)
            {
                Assert.Fail(lastException.ToString() + "\r\n" + internalLog);
            }
        }

        [TestMethod]
        public void AsyncTargetWrapperCloseTest()
        {
            var myTarget = new MyAsyncTarget
            {
                ThrowExceptions = true,

            };

            var targetWrapper = new AsyncTargetWrapper(myTarget)
            {
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                TimeToSleepBetweenBatches = 1000,
            };

            ((ISupportsInitialize)targetWrapper).Initialize();
            ((ISupportsInitialize)myTarget).Initialize();

            bool continuationHit = false;

            targetWrapper.WriteLogEvent(LogEventInfo.CreateNullEvent(), ex => { continuationHit = true; });

            // quickly close the target before the timer elapses
            ((ISupportsInitialize)targetWrapper).Close();

            // continuation will not be hit because the thread is down.
            Thread.Sleep(1000);
            Assert.IsFalse(continuationHit);
        }

        [TestMethod]
        public void AsyncTargetWrapperExceptionTest()
        {
            var targetWrapper = new AsyncTargetWrapper
            {
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                TimeToSleepBetweenBatches = 500,
            };

            ((ISupportsInitialize)targetWrapper).Initialize();

            // don't set wrapped taret - will cause exception on the timer thread
            string internalLog = RunAndCaptureInternalLog(
                () =>
                {
                    targetWrapper.WriteLogEvent(LogEventInfo.CreateNullEvent(), ex => { });
                    Thread.Sleep(1000);
                },
                LogLevel.Trace);

            ((ISupportsInitialize)targetWrapper).Close();
            Assert.IsTrue(internalLog.StartsWith("Error Error in lazy writer timer procedure: System.NullReferenceException", StringComparison.Ordinal), internalLog);
        }

        class MyAsyncTarget : Target
        {
            public int FlushCount;
            public int WriteCount;

            protected override void Write(LogEventInfo logEvent)
            {
                throw new NotSupportedException();
            }

            protected override void Write(LogEventInfo logEvent, AsyncContinuation asyncContinuation)
            {
                Assert.IsTrue(this.FlushCount <= this.WriteCount);
                Interlocked.Increment(ref this.WriteCount);
                ThreadPool.QueueUserWorkItem(
                    s =>
                        {
                            if (this.ThrowExceptions)
                            {
                                asyncContinuation(new InvalidOperationException("Some problem!"));
                                asyncContinuation(new InvalidOperationException("Some problem!"));
                            }
                            else
                            {
                                asyncContinuation(null);
                                asyncContinuation(null);
                            }
                        });
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                Interlocked.Increment(ref this.FlushCount);
                ThreadPool.QueueUserWorkItem(
                    s => asyncContinuation(null));
            }

            public bool ThrowExceptions { get; set; }
        }

        class MyTarget : Target
        {
            public int FlushCount { get; set; }
            public int WriteCount { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.IsTrue(this.FlushCount <= this.WriteCount);
                this.WriteCount++;
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                this.FlushCount++;
                asyncContinuation(null);
            }
        }
    }
}
