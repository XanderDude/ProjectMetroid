using Godot;
using System.Collections.Generic;
using System.ComponentModel;
[GlobalClass]
public partial class DropEntry : Resource
{

    public enum Type { Upgrade, Consumables, Health}
    public enum Upgrade { none, wallJump, ravenSlash, mantling, chargeShot, slideBoost, bombArrows, ravenTeleport };
    [Export] public Upgrade upgrade = Upgrade.none;
    [Export] public Type itemType = Type.Upgrade; 
	[Export] public PackedScene itemScene;
	[Export] public float dropRate = 0.0f;
	[Export] public int dropAmount = 0;
    [Export] public int amount = 0;
    [Export] public bool pitydrop = false; 
	
}