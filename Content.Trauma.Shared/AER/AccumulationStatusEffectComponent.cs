using System.Numerics;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

[RegisterComponent, NetworkedComponent]
public sealed partial class AccumulationStatusEffectComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan TimeOfApplication = TimeSpan.Zero;

    [DataField]
    public Dictionary<float, EntityEffect> StartingEffects = new();

    [DataField]
    public HashSet<EntityEffect> FiredEffects = new();
}
