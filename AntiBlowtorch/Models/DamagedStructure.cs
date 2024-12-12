using System;
using UnityEngine;

namespace RestoreMonarchy.AntiBlowtorch.Models;

public class DamagedStructure
{
    public Transform Transform { get; set; }
    public DateTime LastDamageTime { get; set; }
}