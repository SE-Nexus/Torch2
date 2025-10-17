using NLog;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteUtils.Logging
{
    public static class LoggingExtensions
    {
        public static void InfoColor(this Logger _log, string message, Color color)
        {
            LogEventBuilder logEventBuilder = new LogEventBuilder(_log, LogLevel.Info);
            logEventBuilder.Message(message);
            logEventBuilder.Property("Color", color);



            _log.Log(logEventBuilder.LogEvent);



        }
    }
}
