using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace cs2_autoteam
{
    public class cs2_autoteam : BasePlugin, IPluginConfig<AutoTeamConfig>
    {
        public override string ModuleName => "cs2-autoteam";
        public override string ModuleVersion => "0.0.3";
        public override string ModuleAuthor => "varkit";

        public AutoTeamConfig Config { get; set; }
        public string prefix { get; set; }
        public int allowedteam = 2;

        public override void Load(bool hotReload)
        {
            AddCommandListener("jointeam", OnJoinTeam);
        }

        public void OnConfigParsed(AutoTeamConfig config)
        {
            Config = config;
            prefix = config.Prefix.ReplaceColorTags();
            allowedteam = config.TeamToBeAssigned;
        }

        public HookResult OnJoinTeam(CCSPlayerController? player, CommandInfo command)
        {
            if (!Config.DisableJoinFromTeamMenu) return HookResult.Continue;
            try
            {
                if (player == null || !player.IsValid || player.SteamID == 0)
                {
                    return HookResult.Continue;
                }
                if (command == null || command.ArgCount < 2)
                {
                    return HookResult.Continue;
                }

                string teamArg = command.GetArg(1);
                if (!int.TryParse(teamArg, out int teamNum))
                {
                    return HookResult.Continue;
                }


                if(teamNum != allowedteam)
                {
                    reply(player, Localizer["ChangeTeamError"]);
                    return HookResult.Stop;
                }
                else
                {
                    return HookResult.Continue;
                }
            }
            catch (Exception)
            {
                return HookResult.Continue;
            }
        }

        [GameEventHandler]
        public HookResult playerconnect(EventPlayerConnectFull @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || player.SteamID == 0)
            {
                return HookResult.Continue;
            }
            Server.NextFrame(() =>
            {
                player.ChangeTeam((CsTeam)allowedteam);
            });
            return HookResult.Continue;
        }

        public void reply(CCSPlayerController player, string m)
        {
            player.PrintToChat(prefix + m);
        }
    }

    public class AutoTeamConfig : BasePluginConfig
    {
        [JsonPropertyName("Prefix")]
        public string Prefix { get; set; } = "{red}[AutoTeam]";

        [JsonPropertyName("TeamToBeAssigned")]
        public int TeamToBeAssigned { get; set; } = 2;

        [JsonPropertyName("DisableJoinFromTeamMenu")]
        public bool DisableJoinFromTeamMenu { get; set; } = true;
    }
}
