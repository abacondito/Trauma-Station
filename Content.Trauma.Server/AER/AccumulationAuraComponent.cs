using System.Runtime.CompilerServices;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Physics;

//maybe i should move this aura stuff somewhere else

namespace Content.Trauma.Server.Aer;

[RegisterComponent]
public sealed partial class AccumulationAuraComponent : Component
{
    [DataField]
    public float Range = 2f;

    [DataField]
    public float TickRate = 1f;

    // Tracks how long each entity has been inside the aura
    [DataField]
    public Dictionary<EntityUid, float> Accumulated = new();

    //effects and relative thresholds to activate
    [DataField]
    public Dictionary<EntProtoId, float> StartingEffects = new();

    public Dictionary<EntityUid, HashSet<EntProtoId>> FiredEffects = new();
}