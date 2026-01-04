using NLog;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;
using Color = Spectre.Console.Color;

namespace InstanceUtils.Logging
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

        public static void NoConsole(this Logger _log, LogLevel lvl, string message)
        {
            LogEventBuilder logEventBuilder = new LogEventBuilder(_log, lvl);
            logEventBuilder.Message(message);
            logEventBuilder.Property("NoConsole", true);


            _log.Log(logEventBuilder.LogEvent);
        }
    }
}
