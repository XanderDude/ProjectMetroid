using Godot;
using System;
using System.Linq;

public partial class DeathState : PlayerState
{
    private bool deathEffectApplied = false;
    public override void Enter()
    {
            
        SoundFriend.Play("player_death_SFX");
        //ProcessMode = ProcessModeEnum.Always;
        parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Death();
        player.Velocity = Vector3.Zero;
        player.MoveAndSlide();
        
        PauseGame();
        
        if (!deathEffectApplied)
        {
            ApplyDeathEffect();
            deathEffectApplied = true;
        }
    }
   
    public override void Exit()
    {
        //GD.Print("Exited Death State");
        
        UnpauseGame();
        ProcessMode = Node.ProcessModeEnum.Inherit;
        
        deathEffectApplied = false;
        RestoreVisuals();
    }


   
    public override void HandleInput(InputEvent @event)
    {
    }
   
    private void PauseGame()
    {
    
       GetTree().Paused = true;
		
		// Find and pause the specific SubViewport structure
		var subViewportContainer = GetTree().CurrentScene?.FindChild("SubViewportContainer", true, false);
		if (subViewportContainer == null)
		{
			// Search everywhere for SubViewportContainer
			subViewportContainer = FindNodeByName("SubViewportContainer");
		}
		
		if (subViewportContainer != null)
		{
			var subViewport = subViewportContainer.FindChild("SubViewport", false, false) as SubViewport;
			if (subViewport != null)
			{
				subViewport.ProcessMode = Node.ProcessModeEnum.Disabled;
				
			}
			else
			{
			
			}
		}
		else
		{
		
		}
    }

    private void UnpauseGame()
    {
        GetTree().Paused = false;
        
        var subViewportContainer = GetTree().GetFirstNodeInGroup("world")?.FindChild("SubViewportContainer", true, false);
        if (subViewportContainer == null)
        {
            subViewportContainer = FindNodeByName("SubViewportContainer");
        }
        
        if (subViewportContainer != null)
        {
            var subViewport = subViewportContainer.FindChild("SubViewport", false, false) as SubViewport;
            if (subViewport != null)
            {
                subViewport.ProcessMode = Node.ProcessModeEnum.Inherit;
            }
        }
    }

    private Node FindNodeByName(string name)
    {
        return GetTree().GetNodesInGroup("default").FirstOrDefault(n => n.Name == name);
    }
   
    private void ApplyDeathEffect()
    {
        void MakeNodeBlack(Node node)
    {
        if (node is MeshInstance3D mesh)
        {
            var material = new StandardMaterial3D();
            material.AlbedoColor = Colors.Black;
            material.Emission = Colors.Black;
            mesh.MaterialOverride = material;
        }
        else if (node is GpuParticles3D particles)
        {
            particles.Visible = false; 
        }
        else if (node is CpuParticles3D cpuParticles)
        {
            cpuParticles.Visible = false;
        }
        else if (node is Light3D light)
        {
            light.Visible = false; 
        }
        
        foreach (Node child in node.GetChildren())
            MakeNodeBlack(child);
    }
       
        void MakeNodeWhite(Node node)
        {
            if (node is MeshInstance3D mesh)
            {
                var material = new StandardMaterial3D();
                material.AlbedoColor = Colors.White;
                material.EmissionEnabled = true;
                material.Emission = Colors.White;
                mesh.MaterialOverride = material;
            }
            foreach (Node child in node.GetChildren())
                MakeNodeWhite(child);
        }
       
        MakeNodeBlack(GetTree().CurrentScene);
        MakeNodeWhite(parentMesh);
    }
   
    private void RestoreVisuals()
    {
        void RestoreMaterials(Node node)
        {
            if (node is MeshInstance3D mesh)
            {
                mesh.MaterialOverride = null;
            }
            foreach (Node child in node.GetChildren())
                RestoreMaterials(child);
        }
       
        RestoreMaterials(GetTree().CurrentScene);
    }
}