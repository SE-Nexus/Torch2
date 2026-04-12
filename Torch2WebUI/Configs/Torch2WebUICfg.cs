using Torch2API.Attributes;
using Torch2API.Utils;
using YamlDotNet.Serialization;

namespace Torch2WebUI.Configs
{
    public class Torch2WebUICfg : ConfigBase<Torch2WebUICfg>
    {
        #region Yaml Groups

        public class LoggingConfig
        {
            [EnvVar("TORCH2_LOG_ENABLE_FILE")]
            [YamlMember(Description = "Enable file logging")]
            public bool EnableFileLogging { get; set; } = true;

            [EnvVar("TORCH2_LOG_DIRECTORY")]
            [YamlMember(Description = "Log file directory path")]
            public string LogDirectory { get; set; } = "Logs";

            [EnvVar("TORCH2_LOG_MAX_SIZE_MB")]
            [YamlMember(Description = "Maximum log file size in MB before rotation")]
            public int MaxLogFileSizeMB { get; set; } = 10;

            [EnvVar("TORCH2_LOG_MAX_FILES")]
            [YamlMember(Description = "Maximum number of log files to keep")]
            public int MaxLogFiles { get; set; } = 7;

            [EnvVar("TORCH2_LOG_MAX_AGE_DAYS")]
            [YamlMember(Description = "Maximum age of log files in days before deletion")]
            public int MaxLogAgeDays { get; set; } = 30;

            [EnvVar("TORCH2_LOG_LEVEL")]
            [YamlMember(Description = "Log level (Trace, Debug, Information, Warning, Error, Critical)")]
            public string LogLevel { get; set; } = "Information";

            [EnvVar("TORCH2_LOG_INCLUDE_TIMESTAMPS")]
            [YamlMember(Description = "Include timestamps in log files")]
            public bool IncludeTimestamps { get; set; } = true;

            [EnvVar("TORCH2_LOG_INSTANCES_CONSOLE")]
            [YamlMember(Description = "Log instances logs to console")]
            public bool EnableInstanceLogging { get; set; } = true;
        }

        #endregion


        [EnvVar("TORCH2_PANEL_NAME")]
        [YamlMember(Description = "Name of the Web UI Panel")]
        public string PanelName { get; set; } = "Torch2 Web UI";

        [EnvVar("TORCH2_WEB_PORT")]
        [YamlMember(Description = "Web UI Port")]
        public int Port { get; set; } = 7076;

        [EnvVar("TORCH2_MAX_CONNECTIONS")]
        [YamlMember(Description = "Maximum concurrent connections")]
        public int MaxConnections { get; set; } = 100;

        [YamlMember(Description = "Logging Configuration")]
        public LoggingConfig Logging { get; set; } = new LoggingConfig();
    }
}
