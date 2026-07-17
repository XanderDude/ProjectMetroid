using Godot;
using System;
using System.Linq;

public partial class deathState : PlayerState
{
    private bool deathEffectApplied = false;
    [Export] public float deathDuration = 2.5f;
    private int deathToken = 0;

    public override void Enter()
    {

        SoundFriend.Play("player_death_SFX");
        //ProcessMode = ProcessModeEnum.Always;
        pm.pah.Death();
        pm.Velocity = Vector3.Zero;
        pm.MoveAndSlide();

        PauseGame();

        if (!deathEffectApplied)
        {
            ApplyDeathEffect();
            deathEffectApplied = true;
        }

        // A fixed timer instead of the Death animation's AnimationFinished signal - SceneTreeTimer
        // keeps counting even while GetTree().Paused is true, and doesn't depend on whether the
        // animation resource loops (which would mean AnimationFinished never fires at all).
        deathToken++;
        int myToken = deathToken;
        GetTree().CreateTimer(deathDuration).Timeout += () => OnDeathTimerFinished(myToken);
    }

    private void OnDeathTimerFinished(int token)
    {
        if (token != deathToken) return; // a newer death cycle already started; ignore this stale timer
        if (pm.psm.current_node_state_name != "deathState") return;
        pm.health = pm.maxHealth;
        pm.psm.transition_to("groundedState");
    }

    public override void Exit()
    {
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
				// Deferred: this can run mid-physics-step (e.g. from an enemy collision callback),
				// and Godot disallows disabling a CollisionObject's process mode synchronously there.
				subViewport.CallDeferred(Node.MethodName.SetProcessMode, (int)Node.ProcessModeEnum.Disabled);

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
                subViewport.CallDeferred(Node.MethodName.SetProcessMode, (int)Node.ProcessModeEnum.Inherit);
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
        MakeNodeWhite(pm.playerMesh);
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
