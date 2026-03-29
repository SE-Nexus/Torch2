using InstanceUtils.Services;
using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using System;
using System.Threading.Tasks;
using Torch2API.DTOs.Chat;
using VRage.Game.ModAPI;

namespace IgniteSE1.Services
{
    public class GameChatService : ServiceBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public event Action<ChatMessage> OnChatReceived;

        public GameChatService()
        {
        }

        public override void ServerStarted()
        {
            if (MyMultiplayer.Static is MyMultiplayerBase mp)
            {
                mp.ChatMessageReceived += OnGameChatMessage;
                _logger.Info("GameChatManager subscribed to game chat.");
            }
            else
            {
                _logger.Warn("MyMultiplayer.Static is not available; chat interception disabled.");
            }
        }

        public override Task<bool> ServerStopping()
        {
            if (MyMultiplayer.Static is MyMultiplayerBase mp)
            {
                mp.ChatMessageReceived -= OnGameChatMessage;
                _logger.Info("GameChatManager unsubscribed from game chat.");
            }

            return Task.FromResult(true);
        }

        private void OnGameChatMessage(ulong steamId, string message, ChatChannel channel, long target, VRage.GameServices.ChatMessageCustomData? arg5)
        {
            string displayName = GetPlayerName(steamId);
            string targetDisplay = ResolveTarget(channel, target);

            var chatMsg = new ChatMessage
            {
                SteamId = steamId,
                DisplayName = displayName,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Channel = channel.ToString(),
                Target = targetDisplay
            };

            _logger.Info("[Chat] [{0}] {1}{2}: {3}",
                channel,
                displayName,
                string.IsNullOrEmpty(targetDisplay) ? "" : $" → {targetDisplay}",
                message);

            OnChatReceived?.Invoke(chatMsg);
        }

        private static string ResolveTarget(ChatChannel channel, long target)
        {
            if (target == 0)
                return null;

            switch (channel)
            {
                case ChatChannel.Faction:
                    var faction = MySession.Static?.Factions?.TryGetFactionById(target);
                    if (faction != null)
                        return $"[{faction.Tag}]";
                    return target.ToString();

                case ChatChannel.Private:
                    var identity = MySession.Static?.Players?.TryGetIdentity(target);
                    if (identity != null)
                        return identity.DisplayName;
                    return target.ToString();

                default:
                    return null;
            }
        }

        private static string GetPlayerName(ulong steamId)
        {
            var identity = MySession.Static?.Players?.TryGetIdentityId(steamId);
            if (identity.HasValue && identity.Value != 0)
            {
                var id = MySession.Static?.Players?.TryGetIdentity(identity.Value);
                if (id != null)
                    return id.DisplayName;
            }

            return steamId == 0 ? "Server" : steamId.ToString();
        }
    }
}
