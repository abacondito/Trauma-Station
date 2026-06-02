using Content.Shared.Mind.Components;
using Content.Shared.Emoting;
using Robust.Shared.Audio.Systems;
using Content.Shared.Chat;
using Content.Shared.StatusEffectNew;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;
using Robust.Shared.Timing;
using Content.Shared.EntityEffects;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Content.Shared.Physics;




namespace Content.Trauma.Shared.Aer;

public sealed partial class ApplyStatusEffectsNearSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private FixtureSystem _fixture = default!;
    [Dependency] private EmoteSystem _emote = default!;

    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    [Dependency] private SharedEntityEffectsSystem _entityEffectSystem = default!;

    private HashSet<Entity<MindContainerComponent>> _players = new HashSet<Entity<MindContainerComponent>>();

    /*public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<ApplyStatusEffectsNearComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var aura))
        {
            var center = _transform.GetMapCoordinates(uid);

            _players.Clear();

            var nowInside = new HashSet<EntityUid>();



            _lookup.GetEntitiesInRange(center, aura.Range, _players);

            foreach (var ent in _players)
            {
                if (ent.Owner == uid)
                    continue;

                //check if entity is a player
                if (!HasComp<MindContainerComponent>(ent))
                    continue;

                nowInside.Add(ent.Owner);

                if (!aura.EntitiesNear.Contains(ent.Owner))
                {
                    aura.EntitiesNear.Add(ent.Owner);
                    _statusEffects.TryAddStatusEffectDuration(ent.Owner, aura.Effect, TimeSpan.FromSeconds(31));
                }
            }

            // Handle removals (reset timer if they leave range)
            var toRemove = new List<EntityUid>();

            foreach (var tracked in aura.EntitiesNear)
            {
                if (!nowInside.Contains(tracked))
                    toRemove.Add(tracked);
            }

            foreach (var rem in toRemove)
            {
                aura.EntitiesNear.Remove(rem);
                _statusEffects.TryRemoveStatusEffect(rem, aura.Effect);
            }
        }
    }*/

    public override void Initialize()
    {
        SubscribeLocalEvent<ApplyStatusEffectsNearComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<ApplyStatusEffectsNearComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ApplyStatusEffectsNearComponent, EndCollideEvent>(OnCollideEnd);
    }

    private void OnMapInit(EntityUid uid, ApplyStatusEffectsNearComponent component, MapInitEvent args)
    {

        // Sets up a fixture for flammable collisions.
        // TODO: Should this be generalized into a general non-hard 'effects' fixture or something? I can't think of other use cases for it.
        // This doesn't seem great either (lots more collisions generated) but there isn't a better way to solve it either that I can think of.

        if (!TryComp<PhysicsComponent>(uid, out var body))
            return;

        _fixture.TryCreateFixture(uid, component.CircleCollisionShape, "statusEffectArea", density: 0,
            hard: false, collisionMask: (int) CollisionGroup.FullTileLayer, body: body);
        component.EntitiesInside.Clear();
    }

    private void OnCollide(EntityUid uid, ApplyStatusEffectsNearComponent component, ref StartCollideEvent args)
    {
        var otherUid = args.OtherEntity;


        if (!TryComp(otherUid, out MindContainerComponent? mind) || !mind.HasMind)
            return;

        if (!component.EntitiesInside.Contains(otherUid))
        {
            _statusEffects.TryAddStatusEffectDuration(otherUid, component.Effect, TimeSpan.FromSeconds(31));
            component.EntitiesInside.Add(otherUid);
        }
    }

    private void OnCollideEnd(EntityUid uid, ApplyStatusEffectsNearComponent component, ref EndCollideEvent args)
    {
        var otherUid = args.OtherEntity;

        if (component.EntitiesInside.Contains(otherUid))
        {
            _statusEffects.TryRemoveStatusEffect(otherUid, component.Effect);
            component.EntitiesInside.Remove(otherUid);
        }
    }
}