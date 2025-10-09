using HarmonyLib;
using NLog;
using Sandbox.Engine.Multiplayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRage.Utils;
using VRage.Utils.Keen;

namespace IgniteSE1.Patches
{
    /// <summary>
    /// Largely copied from SE1 to log keen log to NLog Target
    /// </summary>
    [HarmonyPatch]
    public class KeenLogPatch
    {
        private static Logger _log = NLog.LogManager.GetLogger("Keen");


        //Prevent keen log from creating its own log files
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyLogKeen), "InitWithDateNoCheck")]
        private static bool InitWithDateNoCheck(MyLogKeen __instance, string logNameBaseName, StringBuilder appVersionString, int maxLogAgeInDays)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyLog), "Log", new Type[] { typeof(MyLogSeverity), typeof(StringBuilder) })]
        private static bool PrefixLogStringBuilder(MyLog __instance, MyLogSeverity severity, StringBuilder builder)
        {
            _log.Log(LogLevelFor(severity), PrepareLog(__instance).Append(builder));
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyLog), "Log", new Type[] { typeof(MyLogSeverity), typeof(string), typeof(object[])})]
        private static bool PrefixLogFormatted(MyLog __instance, MyLogSeverity severity, string format, object[] args)
        {
            // Sometimes this is called with a pre-formatted string and no args
            // and causes a crash when the format string contains braces
            var sb = PrepareLog(__instance);
            if (args != null && args.Length > 0)
                sb.AppendFormat(format, args);
            else
                sb.Append(format);

            _log.Log(LogLevelFor(severity), sb);
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyLog), nameof(MyLog.WriteLine), new Type[] { typeof(string) })]
        private static bool PrefixWriteLine(MyLog __instance, string msg)
        {
            var log = PrepareLog(__instance).Append(msg);
            _log.Debug(log);
            Trace.WriteLine(log);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyLog), nameof(MyLog.AppendToClosedLog), new Type[] { typeof(string) })]
        private static bool PrefixAppendToClosedLog(MyLog __instance, string text)
        {
            _log.Info(PrepareLog(__instance).Append(text));
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyLog), nameof(MyLog.WriteLine), new Type[] { typeof(string), typeof(LoggingOptions) })]
        private static bool PrefixWriteLineOptions(MyLog __instance, string message, LoggingOptions option)
        {
            var logFlagMethod = typeof(MyLog).GetMethod("LogFlag", BindingFlags.Instance | BindingFlags.NonPublic);

            if (logFlagMethod == null)
                throw new Exception("Failed to find LogFlag method");

            var logFlag = (bool)logFlagMethod.Invoke(__instance, new object[] { option });

            if (logFlag)
                _log.Info(PrepareLog(__instance).Append(message));
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyLog), nameof(MyLog.WriteLine), new Type[] { typeof(Exception) })]
        private static bool PrefixWriteLineException(Exception ex)
        {
            LogException(ex);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyLog), nameof(MyLog.AppendToClosedLog), new Type[] { typeof(Exception) })]
        private static bool PrefixAppendToClosedLogException(Exception e)
        {
            LogException(e);
            return false;
        }

        private static void LogException(Exception ex)
        {
            _log.Error(ex);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyLog), nameof(MyLog.WriteLineAndConsole), new Type[] { typeof(string) })]
        private static bool PrefixWriteLineConsole(MyLog __instance, string msg)
        {
            _log.Info(PrepareLog(__instance).Append(msg));
            return false;
        }


        //[HarmonyPatch(typeof(MyMultiplayerServerBase), nameof(MyMultiplayerServerBase.ValidationFailed), new Type[] { typeof(ulong), typeof(bool), typeof(string), typeof(bool) })]




        private static LogLevel LogLevelFor(MyLogSeverity severity)
        {
            switch (severity)
            {
                case MyLogSeverity.Debug:
                    return LogLevel.Debug;
                case MyLogSeverity.Info:
                    return LogLevel.Info;
                case MyLogSeverity.Warning:
                    return LogLevel.Warn;
                case MyLogSeverity.Error:
                    return LogLevel.Error;
                case MyLogSeverity.Critical:
                    return LogLevel.Fatal;
                default:
                    return LogLevel.Info;
            }
        }

        [ThreadStatic]
        private static StringBuilder _tmpStringBuilder;
        private static StringBuilder PrepareLog(MyLog log)
        {
            if (_tmpStringBuilder == null)
                _tmpStringBuilder = new StringBuilder();

            _tmpStringBuilder.Clear();
            var i = Thread.CurrentThread.ManagedThreadId;
            int t = 0;


            if (log.LogEnabled)
            {
                try
                {
                    t = (int)AccessTools.Method(typeof(MyLog), "GetIdentByThread").Invoke(log, new object[] { i });
                }
                catch (Exception e)
                {
                    //Commented out as it eventually resolves once fully loaded.
                    //_log.Debug(e, "Failed to get thread indent");
                }
            }

            

            _tmpStringBuilder.Append(' ', t * 3);
            return _tmpStringBuilder;
        }



    }
}
