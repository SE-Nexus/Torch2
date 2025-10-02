using IgniteSE1.Utilities;
using NLog;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IgniteSE1.Services
{
    public class SteamService : ServiceBase
    {
        private const int SpaceEngineersAppID = 298740;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IHttpClientFactory _httpClientFactory;
        private ConfigService _configs;


        private string _SteamCMDDir;
        private string _GameInstallDir;


        public SteamService(IHttpClientFactory httpClientFactory, ConfigService configs)
        {
            _httpClientFactory = httpClientFactory;
            _configs = configs;

            _SteamCMDDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configs.Config.Directories.SteamCMDFolder);
            _GameInstallDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configs.Config.Directories.Game);
        }



        public override async Task<bool> Init()
        {
            Directory.CreateDirectory(_SteamCMDDir);


            if (!IsSteamCmdInstalled())
            {
                if (!await DownloadSteamCmd())
                    return false;
            }

            if (!await InstallGame(SpaceEngineersAppID))
            {
                _logger.Error("Failed to install or update Space Engineers server.");
                return false;
            }

            _logger.Info("SteamService initialized.");



            return true;
        }

        private bool IsSteamCmdInstalled()
        {
            return File.Exists(Path.Combine(_SteamCMDDir, "steamcmd.exe"));
        }


        private async Task<bool> DownloadSteamCmd()
        {
            HttpClient _httpClient = _httpClientFactory.CreateClient();
            bool success = false;

            await AnsiConsole.Status()
                        .AutoRefresh(true)
                        .Spinner(Spinner.Known.Dots2)
                        .SpinnerStyle(Style.Parse("yellow"))
                        .StartAsync("Downloading SteamCMD...", async ctx =>
                        {
                            Uri uri = new Uri("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip");
                            var zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp.zip");

                            // Retry up to 3 times
                            const int maxRetries = 20;
                            int attempt = 0;

                            while (attempt < maxRetries && !success)
                            {
                                try
                                {
                                    attempt++;

                                    using (var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                                    {
                                        response.EnsureSuccessStatusCode();

                                        using (var stream = await response.Content.ReadAsStreamAsync())
                                        using (var fileStream = File.Create(zipPath))
                                        {
                                            await stream.CopyToAsync(fileStream);
                                        }
                                    }

                                    success = true; // Download completed
                                }
                                catch (Exception ex)
                                {
                                    ctx.Status($"Attempt {attempt} failed: {ex.Message}");

                                    if (attempt >= maxRetries)
                                    {
                                        ctx.Status("Download failed after multiple attempts. Exiting safely.");
                                        return; // or Environment.Exit(1) if you want a hard exit
                                    }

                                    // Optional: wait a bit before retrying
                                    await Task.Delay(5000);
                                }
                            }

                            ctx.Status("Extracting SteamCMD...");

                            // Now extract
                            ZipFile.ExtractToDirectory(zipPath, _SteamCMDDir);

                            ctx.Status("Cleaning up temporary files...");

                            File.Delete(zipPath); // Clean up the temporary ZIP file

                        });

            return success;
        }

        private async Task<bool> InstallGame(int appId)
        {

            bool isEmpty = !Directory.Exists(_GameInstallDir) || !Directory.EnumerateFileSystemEntries(_GameInstallDir).Any();
            bool RedirectStandardOutput = true; //Set to false to see the output in the console

            if (isEmpty)
            {
                _logger.InfoColor($"Installing server with AppID: {appId} to {_GameInstallDir}", Color.Orange1);
                Directory.CreateDirectory(_GameInstallDir);
                RedirectStandardOutput = false;
            }

            //Additional check to see if the game is already installed
            bool NeedValidate = false;


            //Start the process to install the game using SteamCMD
            var processStartInfo = new ProcessStartInfo("cmd.exe")
            {
                Arguments = $"/c \"steamcmd +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +force_install_dir \"{_GameInstallDir}\" +login anonymous +app_update {appId} +quit\"",
                WorkingDirectory = _SteamCMDDir,
                RedirectStandardOutput = RedirectStandardOutput,
                UseShellExecute = NeedValidate
            };


            using (var cmdProcess = new Process { StartInfo = processStartInfo })
            {


                cmdProcess.Start();


                if (RedirectStandardOutput)
                {
                    await AnsiConsole.Status()
                        .AutoRefresh(true)
                        .Spinner(Spinner.Known.Dots2)
                        .SpinnerStyle(Style.Parse("yellow"))
                        .StartAsync("Logging into steam... ", async ctx =>
                        {
                            var processTask = WaitForExitAsync(cmdProcess);
                            var delayTask = Task.Delay(TimeSpan.FromSeconds(10));

                            var completed = await Task.WhenAny(processTask, delayTask);

                            // Task to update status text after 10 seconds
                            if (completed == delayTask)
                            {
                                // 10 seconds passed, process is still running
                                ctx.Status("Steam not responding...");
                            }


                            // Always await the process to fully finish
                            await processTask;
                        });
                }
                else
                {
                    await WaitForExitAsync(cmdProcess);
                }

                return cmdProcess.ExitCode == 0;
            }
        }

        public static Task WaitForExitAsync(Process process, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<object>();

            void Handler(object sender, EventArgs args)
            {
                process.Exited -= Handler;
                tcs.TrySetResult(null);
            }

            process.EnableRaisingEvents = true;
            process.Exited += Handler;

            if (cancellationToken != default)
            {
                cancellationToken.Register(() =>
                {
                    process.Exited -= Handler;
                    tcs.TrySetCanceled();
                });
            }

            if (process.HasExited) // in case it already finished
            {
                process.Exited -= Handler;
                tcs.TrySetResult(null);
            }

            return tcs.Task;
        }

    }
}
