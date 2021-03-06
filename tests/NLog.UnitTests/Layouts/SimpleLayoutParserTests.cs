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

namespace NLog.UnitTests.Layouts
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.LayoutRenderers;
    using NLog.LayoutRenderers.Wrappers;
    using NLog.Layouts;

    [TestClass]
    public class SimpleLayoutParserTests : NLogTestBase
    {
        [TestMethod]
        public void SimpleTest()
        {
            SimpleLayout l = "${message}";
            Assert.AreEqual(1, l.Renderers.Count);
            Assert.IsInstanceOfType(l.Renderers[0], typeof(MessageLayoutRenderer));
        }

        [TestMethod]
        public void UnclosedTest()
        {
            Layout l = "${message";
        }

        [TestMethod]
        public void SingleParamTest()
        {
            SimpleLayout l = "${mdc:item=AAA}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA", mdc.Item);
        }

        [TestMethod]
        public void ValueWithColonTest()
        {
            SimpleLayout l = "${mdc:item=AAA\\:}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA:", mdc.Item);
        }

        [TestMethod]
        public void ValueWithBracketTest()
        {
            SimpleLayout l = "${mdc:item=AAA\\}\\:}";
            Assert.AreEqual("${mdc:item=AAA\\}\\:}", l.Text);
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA}:", mdc.Item);
        }

        [TestMethod]
        public void DefaultValueTest()
        {
            SimpleLayout l = "${mdc:BBB}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("BBB", mdc.Item);
        }

        [TestMethod]
        public void DefaultValueWithBracketTest()
        {
            SimpleLayout l = "${mdc:AAA\\}\\:}";
            Assert.AreEqual(l.Text, "${mdc:AAA\\}\\:}");
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA}:", mdc.Item);
        }

        [TestMethod]
        public void DefaultValueWithOtherParametersTest()
        {
            SimpleLayout l = "${exception:message,type:separator=x}";
            Assert.AreEqual(1, l.Renderers.Count);
            ExceptionLayoutRenderer elr = l.Renderers[0] as ExceptionLayoutRenderer;
            Assert.IsNotNull(elr);
            Assert.AreEqual("message,type", elr.Format);
            Assert.AreEqual("x", elr.Separator);
        }

        [TestMethod]
        public void EmptyValueTest()
        {
            SimpleLayout l = "${mdc:item=}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("", mdc.Item);
        }

        [TestMethod]
        public void NestedLayoutTest()
        {
            SimpleLayout l = "${rot13:inner=${ndc:topFrames=3:separator=x}}";
            Assert.AreEqual(1, l.Renderers.Count);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.IsNotNull(lr);
            var nestedLayout = lr.Inner as SimpleLayout;
            Assert.IsNotNull(nestedLayout);
            Assert.AreEqual("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.AreEqual(1, nestedLayout.Renderers.Count);
            var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
            Assert.IsNotNull(ndcLayoutRenderer);
            Assert.AreEqual(3, ndcLayoutRenderer.TopFrames);
            Assert.AreEqual("x", ndcLayoutRenderer.Separator);
        }

        [TestMethod]
        public void DoubleNestedLayoutTest()
        {
            SimpleLayout l = "${rot13:inner=${rot13:inner=${ndc:topFrames=3:separator=x}}}";
            Assert.AreEqual(1, l.Renderers.Count);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.IsNotNull(lr);
            var nestedLayout0 = lr.Inner as SimpleLayout;
            Assert.IsNotNull(nestedLayout0);
            Assert.AreEqual("${rot13:inner=${ndc:topFrames=3:separator=x}}", nestedLayout0.Text);
            var innerRot13 = nestedLayout0.Renderers[0] as Rot13LayoutRendererWrapper;
            var nestedLayout = innerRot13.Inner as SimpleLayout;
            Assert.IsNotNull(nestedLayout);
            Assert.AreEqual("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.AreEqual(1, nestedLayout.Renderers.Count);
            var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
            Assert.IsNotNull(ndcLayoutRenderer);
            Assert.AreEqual(3, ndcLayoutRenderer.TopFrames);
            Assert.AreEqual("x", ndcLayoutRenderer.Separator);
        }

        [TestMethod]
        public void DoubleNestedLayoutWithDefaultLayoutParametersTest()
        {
            SimpleLayout l = "${rot13:${rot13:${ndc:topFrames=3:separator=x}}}";
            Assert.AreEqual(1, l.Renderers.Count);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.IsNotNull(lr);
            var nestedLayout0 = lr.Inner as SimpleLayout;
            Assert.IsNotNull(nestedLayout0);
            Assert.AreEqual("${rot13:${ndc:topFrames=3:separator=x}}", nestedLayout0.Text);
            var innerRot13 = nestedLayout0.Renderers[0] as Rot13LayoutRendererWrapper;
            var nestedLayout = innerRot13.Inner as SimpleLayout;
            Assert.IsNotNull(nestedLayout);
            Assert.AreEqual("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.AreEqual(1, nestedLayout.Renderers.Count);
            var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
            Assert.IsNotNull(ndcLayoutRenderer);
            Assert.AreEqual(3, ndcLayoutRenderer.TopFrames);
            Assert.AreEqual("x", ndcLayoutRenderer.Separator);
        }

        [TestMethod]
        public void AmbientPropertyTest()
        {
            SimpleLayout l = "${message:padding=10}";
            Assert.AreEqual(1, l.Renderers.Count);
            var pad = l.Renderers[0] as PaddingLayoutRendererWrapper;
            Assert.IsNotNull(pad);
            var message = ((SimpleLayout)pad.Inner).Renderers[0] as MessageLayoutRenderer;
            Assert.IsNotNull(message);
        }

        [TestMethod]
        public void DoubleAmbientPropertyTest()
        {
            SimpleLayout l = "${message:uppercase=true:padding=10}";
            Assert.AreEqual(1, l.Renderers.Count);
            var upperCase = l.Renderers[0] as UpperCaseLayoutRendererWrapper;
            Assert.IsNotNull(upperCase);
            var pad = ((SimpleLayout)upperCase.Inner).Renderers[0] as PaddingLayoutRendererWrapper;
            Assert.IsNotNull(pad);
            var message = ((SimpleLayout)pad.Inner).Renderers[0] as MessageLayoutRenderer;
            Assert.IsNotNull(message);
        }

        [TestMethod]
        public void ReverseDoubleAmbientPropertyTest()
        {
            SimpleLayout l = "${message:padding=10:uppercase=true}";
            Assert.AreEqual(1, l.Renderers.Count);
            var pad = ((SimpleLayout)l).Renderers[0] as PaddingLayoutRendererWrapper;
            Assert.IsNotNull(pad);
            var upperCase = ((SimpleLayout)pad.Inner).Renderers[0] as UpperCaseLayoutRendererWrapper;
            Assert.IsNotNull(upperCase);
            var message = ((SimpleLayout)upperCase.Inner).Renderers[0] as MessageLayoutRenderer;
            Assert.IsNotNull(message);
        }

        [TestMethod]
        public void EscapeTest()
        {
            AssertEscapeRoundTrips(string.Empty);
            AssertEscapeRoundTrips("hello ${${}} world!");
            AssertEscapeRoundTrips("hello $");
            AssertEscapeRoundTrips("hello ${");
            AssertEscapeRoundTrips("hello $${{");
            AssertEscapeRoundTrips("hello ${message}");
            AssertEscapeRoundTrips("hello ${${level}}");
            AssertEscapeRoundTrips("hello ${${level}${message}}");
        }

        [TestMethod]
        public void EvaluateTest()
        {
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Level = LogLevel.Warn;
            Assert.AreEqual("Warn", SimpleLayout.Evaluate("${level}", logEventInfo));
        }

        [TestMethod]
        public void EvaluateTest2()
        {
            Assert.AreEqual("Off", SimpleLayout.Evaluate("${level}"));
            Assert.AreEqual(string.Empty, SimpleLayout.Evaluate("${message}"));
            Assert.AreEqual(string.Empty, SimpleLayout.Evaluate("${logger}"));
        }

        private static void AssertEscapeRoundTrips(string originalString)
        {
            string escapedString = SimpleLayout.Escape(originalString);
            SimpleLayout l = escapedString;
            string renderedString = l.Render(LogEventInfo.CreateNullEvent());
            Assert.AreEqual(originalString, renderedString);
        }
    }
}
