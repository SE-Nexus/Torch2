using NLog;
using NLog.Targets;
using Torch2API.DTOs.Logs;

namespace InstanceUtils.Logging
{
    [Target("WebSocketLog")]
    public sealed class WebSocketLogTarget : TargetWithLayout
    {
        public WebSocketLogTarget()
        {
            Layout = "${message}${onexception:inner= ${exception:format=ToString}}";
        }

        protected override void Write(LogEventInfo logEvent)
        {
            LogBuffer.Instance.Add(new LogLine
            {
                InstanceName = logEvent.LoggerName ?? string.Empty,
                Level        = logEvent.Level.Name,
                Message      = RenderLogEvent(Layout, logEvent),
                Timestamp    = logEvent.TimeStamp.ToUniversalTime()
            });
        }
    }
}
