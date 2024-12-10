using Steamworks;
using System;

namespace RestoreMonarchy.AntiBlowtorch.Models;

public class PlayerMessage
{
    public CSteamID PlayerID { get; set; }
    public DateTime LastMessageTime { get; set; }
}