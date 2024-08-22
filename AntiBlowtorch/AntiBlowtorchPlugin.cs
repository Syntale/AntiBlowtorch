using RestoreMonarchy.AntiBlowtorch.Models;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RestoreMonarchy.AntiBlowtorch;

public class AntiBlowtorchPlugin : RocketPlugin
{
    public static AntiBlowtorchPlugin Instance { get; private set; }
    public static List<DamagedStructures> DamagedStructures = new List<DamagedStructures>();
    public static List<PlayerMessages> PlayerMessages = new List<PlayerMessages>();

    protected override void Load()
    {
        Instance = this;
        StructureManager.onDamageStructureRequested += OnStructureDamaged;
        StructureManager.OnRepairRequested += OnRepairRequest;
        BarricadeManager.onDamageBarricadeRequested += OnStructureDamaged;
        BarricadeManager.OnRepairRequested += OnRepairRequest;
        StructureDrop.OnSalvageRequested_Global += OnSalvageRequest;

        InvokeRepeating(nameof(ClearDamagedStructures), 0, 60);
        InvokeRepeating(nameof(ClearPlayerMessages), 0, 10);

        Logger.Log($"{Name} {Assembly.GetName().Version.ToString(3)} has been loaded!", ConsoleColor.Yellow);
        Logger.Log("Check out more Unturned plugins at restoremonarchy.com");
    }

    protected override void Unload()
    {
        StructureManager.onDamageStructureRequested -= OnStructureDamaged;
        StructureManager.OnRepairRequested -= OnRepairRequest;
        BarricadeManager.onDamageBarricadeRequested -= OnStructureDamaged;
        BarricadeManager.OnRepairRequested -= OnRepairRequest;
        StructureDrop.OnSalvageRequested_Global -= OnSalvageRequest;

        DamagedStructures.Clear();
        PlayerMessages.Clear();

        CancelInvoke(nameof(ClearDamagedStructures));
        CancelInvoke(nameof(ClearPlayerMessages));

        Instance = null;
        Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
    }

    public override TranslationList DefaultTranslations => new()
    {
        { "BlockBlowtorch", "You cannot use blowtorch to repair this {0}, because it was recently damaged. You can repair it in {1} seconds." },
        { "BlockSalvage", "You cannot salvage this {0},  because it was recently damaged. You can salvage it in {1} seconds." }
    };

    private void OnSalvageRequest(StructureDrop structure, SteamPlayer instigatorClient, ref bool shouldAllow)
    {
        int instanceId = structure.model.GetInstanceID();
        DateTime now = DateTime.UtcNow;

        DamagedStructures damagedStructure = DamagedStructures.FirstOrDefault(ds => ds.InstanceID == instanceId);

        if (damagedStructure != null && (now - damagedStructure.LastDamageTime).TotalMinutes <= 5)
        {
            shouldAllow = false;
            UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(instigatorClient);
            double remainingTime = (damagedStructure.LastDamageTime.AddMinutes(5) - now).TotalSeconds;

            PlayerMessages playerMessage = PlayerMessages.FirstOrDefault(pm => pm.PlayerID == player.CSteamID);
            if (playerMessage == null || (now - playerMessage.LastMessageTime).TotalSeconds > 10)
            {
                string message = Translate("BlockSalvage", );
                UnturnedChat.Say(player, $"You cannot salvage this structure as it was recently damaged. You can salvage it in {remainingTime:F0} seconds.");
                if (playerMessage == null)
                {
                    PlayerMessages.Add(new PlayerMessages { PlayerID = player.CSteamID, LastMessageTime = now });
                }
                else
                {
                    playerMessage.LastMessageTime = now;
                }
            }
        }
    }

    private void OnRepairRequest(CSteamID instigatorsteamid, Transform structuretransform, ref float pendingtotalhealing, ref bool shouldallow)
    {
        int instanceid = structuretransform.GetInstanceID();
        Logger.LogWarning($"{instanceid} - Inside RepairRequest");
        DateTime now = DateTime.UtcNow;
        
        
        DamagedStructures damagedStructure = DamagedStructures.FirstOrDefault(ds => ds.InstanceID == instanceid);

        if (damagedStructure != null && (now - damagedStructure.LastDamageTime).TotalMinutes <= 5)
        {
            shouldallow = false;
            pendingtotalhealing = 0;
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorsteamid);
            double remainingTime = (damagedStructure.LastDamageTime.AddMinutes(5) - now).TotalSeconds;


            PlayerMessages playerMessage = PlayerMessages.FirstOrDefault(pm => pm.PlayerID == instigatorsteamid);
            if (playerMessage != null && (now - playerMessage.LastMessageTime).TotalSeconds <= 10)
            {
                return;
            }
            
            UnturnedChat.Say(player, $"You cannot use the blowtorch on this structure as it was recently damaged. You can heal it in {remainingTime:F0} seconds.");
            if (playerMessage == null)
            {
                PlayerMessages.Add(new PlayerMessages { PlayerID = instigatorsteamid, LastMessageTime = now });
            }
            else
            {
                playerMessage.LastMessageTime = now;
            }
        }
    }
    
    private void ClearDamagedStructures()
    {
        DamagedStructures.RemoveAll(ds => (DateTime.UtcNow - ds.LastDamageTime).TotalMinutes > 5);
    }
    
    private void ClearPlayerMessages()
    {
        PlayerMessages.RemoveAll(pm => (DateTime.UtcNow - pm.LastMessageTime).TotalSeconds > 10);
    }

    private void OnStructureDamaged(CSteamID instigatorsteamid, Transform transform, ref ushort pendingtotaldamage, ref bool shouldallow, EDamageOrigin damageorigin)
    {
        int instanceid = transform.GetInstanceID();
        DamagedStructures.Add(new DamagedStructures
        {
            InstanceID = instanceid,
            LastDamageTime = DateTime.UtcNow
        });
    }
}