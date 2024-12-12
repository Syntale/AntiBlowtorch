using RestoreMonarchy.AntiBlowtorch.Models;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RestoreMonarchy.AntiBlowtorch;

public class AntiBlowtorchPlugin : RocketPlugin<AntiBlowtorchConfiguration>
{
    public static AntiBlowtorchPlugin Instance { get; private set; }
    public Color MessageColor { get; set; }

    public static List<DamagedStructure> DamagedStructures = [];
    public static List<PlayerMessage> PlayerMessages = [];

    protected override void Load()
    {
        Instance = this;
        MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, Color.green);

        StructureManager.onDamageStructureRequested += OnStructureDamaged;
        BarricadeManager.onDamageBarricadeRequested += OnBarricadeDamaged;
        StructureManager.OnRepairRequested += OnRepairRequest;
        BarricadeManager.OnRepairRequested += OnRepairRequest;
        StructureDrop.OnSalvageRequested_Global += OnSalvageStructureRequest;
        BarricadeDrop.OnSalvageRequested_Global += OnSalvageBarricadeRequest;

        InvokeRepeating(nameof(ClearDamagedStructures), 300, 300);
        InvokeRepeating(nameof(ClearPlayerMessages), 300, 300);

        Logger.Log($"{Name} {Assembly.GetName().Version.ToString(3)} has been loaded!", ConsoleColor.Yellow);
        Logger.Log("Check out more Unturned plugins at restoremonarchy.com");
    }

    protected override void Unload()
    {
        StructureManager.onDamageStructureRequested -= OnStructureDamaged;
        BarricadeManager.onDamageBarricadeRequested -= OnBarricadeDamaged;
        StructureManager.OnRepairRequested -= OnRepairRequest;
        BarricadeManager.OnRepairRequested -= OnRepairRequest;
        StructureDrop.OnSalvageRequested_Global -= OnSalvageStructureRequest;
        BarricadeDrop.OnSalvageRequested_Global -= OnSalvageBarricadeRequest;

        DamagedStructures.Clear();
        PlayerMessages.Clear();

        CancelInvoke(nameof(ClearDamagedStructures));
        CancelInvoke(nameof(ClearPlayerMessages));

        Instance = null;
        Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
    }

    public override TranslationList DefaultTranslations => new()
    {
        { "BlockRepair", "You can't repair this [[b]]{0}[[/b]], because it was recently damaged. Wait [[b]]{1}[[/b]] seconds." },
        { "BlockSalvage", "You can't salvage this [[b]]{0}[[/b]], because it was recently damaged. Wait [[b]]{1}[[/b]] seconds." }
    };

    private void OnSalvageBarricadeRequest(BarricadeDrop barricade, SteamPlayer instigatorClient, ref bool shouldAllow)
    {
        if (!shouldAllow)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;

        DamagedStructure damagedStructure = DamagedStructures.FirstOrDefault(ds => ds.Transform == barricade.model);
        if (damagedStructure == null)
        {
            return;
        }

        if ((now - damagedStructure.LastDamageTime).TotalSeconds <= Configuration.Instance.BlockTimeSeconds)
        {
            shouldAllow = false;
            UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(instigatorClient);
            double remainingTime = (damagedStructure.LastDamageTime.AddSeconds(Configuration.Instance.BlockTimeSeconds) - now).TotalSeconds;
            string structureName = barricade.asset.itemName;
            string remainingTimeString = remainingTime.ToString("F0");
            SendMessageToPlayer(player, "BlockSalvage", structureName, remainingTimeString);
        }
    }

    private void OnSalvageStructureRequest(StructureDrop structure, SteamPlayer instigatorClient, ref bool shouldAllow)
    {
        if (!shouldAllow)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;

        DamagedStructure damagedStructure = DamagedStructures.FirstOrDefault(ds => ds.Transform == structure.model);
        if (damagedStructure == null)
        {
            return;
        }

        if ((now - damagedStructure.LastDamageTime).TotalSeconds <= Configuration.Instance.BlockTimeSeconds)
        {
            shouldAllow = false;
            UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(instigatorClient);
            double remainingTime = (damagedStructure.LastDamageTime.AddSeconds(Configuration.Instance.BlockTimeSeconds) - now).TotalSeconds;
            string structureName = structure.asset.itemName;
            string remainingTimeString = remainingTime.ToString("F0");
            SendMessageToPlayer(player, "BlockSalvage", structureName, remainingTimeString);
        }
    }

    private void OnRepairRequest(CSteamID instigatorsteamid, Transform transform, ref float pendingTotalHealing, ref bool shouldAllow)
    {
        if (!shouldAllow)
        {
            return;
        }

        int instanceId = transform.GetInstanceID();
        DateTime now = DateTime.UtcNow;

        DamagedStructure damagedStructure = DamagedStructures.FirstOrDefault(ds => ds.Transform == transform);

        if (damagedStructure != null && (now - damagedStructure.LastDamageTime).TotalSeconds <= Configuration.Instance.BlockTimeSeconds)
        {
            shouldAllow = false;
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorsteamid);
            double remainingTime = (damagedStructure.LastDamageTime.AddSeconds(Configuration.Instance.BlockTimeSeconds) - now).TotalSeconds;

            PlayerMessage playerMessage = PlayerMessages.FirstOrDefault(pm => pm.PlayerID == instigatorsteamid);
            if (playerMessage != null && (now - playerMessage.LastMessageTime).TotalSeconds <= Configuration.Instance.MessageThrottleTimeSeconds)
            {
                return;
            }

            string structureName = transform.name;
            if (ushort.TryParse(structureName, out ushort structureId))
            {
                Asset asset = Assets.find(EAssetType.ITEM, structureId);
                if (asset != null)
                {
                    structureName = asset.FriendlyName;
                }
            }
            
            string remainingTimeString = remainingTime.ToString("F0");
            SendMessageToPlayer(player, "BlockRepair", structureName, remainingTimeString);

            if (playerMessage == null)
            {
                PlayerMessages.Add(new PlayerMessage { PlayerID = instigatorsteamid, LastMessageTime = now });
            }
            else
            {
                playerMessage.LastMessageTime = now;
            }
        }
    }

    private void ClearDamagedStructures()
    {
        DamagedStructures.RemoveAll(ds => (DateTime.UtcNow - ds.LastDamageTime).TotalSeconds > Configuration.Instance.BlockTimeSeconds);
    }

    private void ClearPlayerMessages()
    {
        PlayerMessages.RemoveAll(pm => (DateTime.UtcNow - pm.LastMessageTime).TotalSeconds > Configuration.Instance.MessageThrottleTimeSeconds);
    }

    private void OnStructureDamaged(CSteamID instigatorSteamId, Transform structureTransform, ref ushort pendingtotaldamage, ref bool shouldallow, EDamageOrigin damageorigin)
    {
        if (Configuration.Instance.IgnoreOwnerAndGroup)
        {
            StructureDrop drop = StructureManager.FindStructureByRootTransform(structureTransform);
            Player player = PlayerTool.getPlayer(instigatorSteamId);
            if (drop == null || player == null)
            {
                return;
            }
            StructureData structureData = drop.GetServersideData();
            if (instigatorSteamId.m_SteamID == structureData.owner)
            {
                return;
            }
            if (player.channel.owner.playerID.group.m_SteamID == structureData.group)
            {
                return;
            }
        }

        RegisterDamagedStructure(structureTransform);
    }

    private void OnBarricadeDamaged(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
    {
        if (Configuration.Instance.IgnoreOwnerAndGroup)
        {
            BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(barricadeTransform);
            Player player = PlayerTool.getPlayer(instigatorSteamID);
            if (drop == null || player == null)
            {
                return;
            }

            BarricadeData barricadeData = drop.GetServersideData();
            if (instigatorSteamID.m_SteamID == barricadeData.owner)
            {
                return;
            }

            if (player.channel.owner.playerID.group.m_SteamID == barricadeData.group)
            {
                return;
            }
        }        

        RegisterDamagedStructure(barricadeTransform);
    }

    private void RegisterDamagedStructure(Transform transform)
    {
        DamagedStructure damagedStructure = DamagedStructures.FirstOrDefault(ds => ds.Transform == transform);
        if (damagedStructure != null)
        {
            damagedStructure.LastDamageTime = DateTime.UtcNow;
        }
        else
        {
            DamagedStructures.Add(new DamagedStructure
            {
                Transform = transform,
                LastDamageTime = DateTime.UtcNow
            });
        }
    }

    internal void SendMessageToPlayer(IRocketPlayer player, string translationKey, params object[] placeholder)
    {
        string msg = Translate(translationKey, placeholder);
        msg = msg.Replace("[[", "<").Replace("]]", ">");
        if (player is ConsolePlayer)
        {
            Logger.Log(msg);
            return;
        }

        UnturnedPlayer unturnedPlayer = (UnturnedPlayer)player;
        if (unturnedPlayer != null)
        {
            ChatManager.serverSendMessage(msg, MessageColor, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, Configuration.Instance.MessageIconUrl, true);
        }
    }
}