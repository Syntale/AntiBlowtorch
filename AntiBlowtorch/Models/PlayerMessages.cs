using Steamworks;

namespace RestoreMonarchy.AntiBlowtorch.Models;

public class PlayerMessages
{
    public CSteamID PlayerID { get; set; }
    public DateTime LastMessageTime { get; set; }
}