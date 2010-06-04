﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NLog;
using NLog.Targets;
using NLog.Config;


namespace testmsmq
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageQueueTarget target = new MessageQueueTarget();
            target.Queue = ".\\private$\\nlog.${level}";
            target.Label = "${message}";
            target.Layout = "${message}";
            //target.CreateQueueIfNotExists = true;
            //target.Recoverable = true;

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            Logger l = LogManager.GetLogger("AAA");
            for (int k = 0; k < 100; k++)
            {
                l.Error("This is an error. It goes to .\\private$\\nlog.Error queue.");
                l.Debug("This is a debug information. It goes to .\\private$\\nlog.Debug queue.");
                l.Info("This is a information. It goes to .\\private$\\nlog.Info queue.");
                l.Warn("This is a warn information. It goes to .\\private$\\nlog.Warn queue.");
                l.Fatal("This is a fatal information. It goes to .\\private$\\nlog.Fatal queue.");
                l.Trace("This is a trace information. It goes to .\\private$\\nlog.Trace queue.");
            }
        }
    }
}
