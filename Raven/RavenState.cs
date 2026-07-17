using Godot;

public partial class RavenState : State
{
    protected Raven raven => GetParent().GetParent<Raven>();
    protected PlayerManager pm => raven.pm;
}