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

namespace NLog.Targets.Compound
{
    using System;
    using System.Threading;
    using NLog.Internal;

    /// <summary>
    /// A compound target that writes logging events to all attached
    /// sub-targets.
    /// </summary>
    /// <example>
    /// <p>This example causes the messages to be written to both file1.txt or file2.txt 
    /// </p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/SplitGroup/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/SplitGroup/Simple/Example.cs" />
    /// </example>
    [Target("SplitGroup", IsCompound = true)]
    public class SplitGroupTarget : CompoundTargetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplitGroupTarget" /> class.
        /// </summary>
        public SplitGroupTarget()
            : this(new Target[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitGroupTarget" /> class.
        /// </summary>
        /// <param name="targets">The targets.</param>
        public SplitGroupTarget(params Target[] targets)
            : base(targets)
        {
        }

        /// <summary>
        /// Forwards the specified log event to all sub-targets.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void Write(LogEventInfo logEvent, AsyncContinuation asyncContinuation)
        {
            AsyncHelpers.ForEachItemSequentially(this.Targets, asyncContinuation, (t, cont) => t.WriteLogEvent(logEvent, cont));
        }
    }
}
