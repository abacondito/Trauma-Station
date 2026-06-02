using System.Runtime.CompilerServices;
using Content.Shared.EntityEffects;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;

//maybe i should move this aura stuff somewhere else

namespace Content.Trauma.Shared.Aer;

[RegisterComponent]
public sealed partial class ApplyStatusEffectsNearComponent : Component
{
    [DataField]
    public float Range = 2f;

    [DataField]
    public float TickRate = 1f;

    // Tracks how long each entity has been inside the aura
    [DataField]
    public HashSet<EntityUid> EntitiesInside = new();

    //status effect applied by the aura
    [DataField]
    public EntProtoId Effect = new();

    [DataField]
    public IPhysShape CircleCollisionShape = new PhysShapeCircle(2f);
}