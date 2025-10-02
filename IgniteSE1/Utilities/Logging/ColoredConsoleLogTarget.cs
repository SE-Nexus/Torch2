using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Layouts;
using NLog.Targets;

namespace IgniteSE1.Utilities
{
    [Target("ColoredConsole")]
    public class ColoredConsoleLogTarget : TargetWithLayout
    {
        private readonly SimpleLayout prefixLayout;
        private readonly SimpleLayout postfixLayout;
        private readonly SimpleLayout combinedLayout;

        public ColoredConsoleLogTarget()
        {
            string prefix = "${date:format=HH\\:mm\\:ss\\.fff} ${pad:padding=-8:inner=[${level:uppercase=true}]}";
            string postfix = "${logger:shortName=true}: ${message:withException=false}\n";

            prefixLayout = new SimpleLayout(prefix);
            postfixLayout = new SimpleLayout(postfix);
            combinedLayout = new SimpleLayout(prefix + postfix);
        }


        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = Layout.Render(logEvent);
            string prefixText;

            bool hasPostfix = TryGetColor(logEvent, out Color postfixColor);
            string formattedMessage = string.Empty;
            if (!hasPostfix)
                prefixText = combinedLayout.Render(logEvent);
            else
                prefixText = prefixLayout.Render(logEvent);

            AnsiConsole.ResetColors();

            switch (logEvent.Level.Name)
            {
                case "Info":



                    if (hasPostfix)
                    {
                        formattedMessage = $"{Markup.Escape(prefixText)}[#{ConvertBrushToHex(postfixColor)}]{Markup.Escape(postfixLayout.Render(logEvent))}[/]";
                    }
                    else
                    {
                        formattedMessage = $"{Markup.Escape(prefixText)}";
                    }

                    break;

                case "Warn":
                    formattedMessage = $"[yellow]{Markup.Escape(prefixText)}[/]";
                    break;
                case "Error":
                    formattedMessage = $"[red bold]{Markup.Escape(prefixText)}[/]";
                    break;
                case "Debug":
                    formattedMessage = $"[cyan]{Markup.Escape(prefixText)}[/]";
                    break;
                case "Trace":
                    formattedMessage = $"[blue]{Markup.Escape(prefixText)}[/]";
                    break;
                case "Fatal":
                    formattedMessage = $"[bold underline red]{Markup.Escape(prefixText)}[/]";
                    break;


                default:
                    Markup.Escape(logMessage);
                    break;
            }

            AnsiConsole.Markup(formattedMessage);
            if (logEvent.Exception != null)
            {
                AnsiConsole.WriteException(logEvent.Exception, new ExceptionSettings
                {
                    Format = ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes,
                    Style = new ExceptionStyle
                    {
                        Exception = new Style().Foreground(Color.Red),
                    }
                });
            }
        }

        private string ConvertBrushToHex(Color brush)
        {
            return brush.ToHex(); // Default to white if the brush is not a SolidColorBrush
        }

        static string EnsureEndsWithNewline(string input)
        {
            if (!input.EndsWith(Environment.NewLine))
            {
                input += Environment.NewLine;
            }
            return input;
        }

        public static bool TryGetColor(LogEventInfo logEvent, out Color color)
        {

            if (logEvent.Properties != null && logEvent.Properties.Count > 0)
            {
                var firstArg = logEvent.Properties.First();
                if (firstArg.Value is Color logColor)
                {
                    color = logColor;
                    return true;
                }
            }

            color = default;
            return false;
        }

    }
}
