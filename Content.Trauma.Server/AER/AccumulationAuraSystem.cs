using Content.Shared.Mobs.Components;
using Robust.Shared.GameObjects;
using Content.Trauma.Server.Aer;
using Robust.Shared.Player;
using Content.Shared.Mind.Components;
using Content.Shared.Emoting;
using Robust.Shared.Audio.Systems;
using NetCord;
using Content.Shared.Chat;
using System.Runtime.CompilerServices;
using Content.Trauma.Shared.StatusEffects;
using Content.Shared.StatusEffectNew;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;


namespace Content.Trauma.Server.Aer;

public sealed partial class AccumulationAuraSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EmoteSystem _emote = default!;

    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private EntityManager _entityManager = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;


    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<AccumulationAuraComponent>();


        while (query.MoveNext(out var uid, out var aura))
        {
            var center = _transform.GetMapCoordinates(uid);

            var nowInside = new HashSet<EntityUid>();

            foreach (var ent in _lookup.GetEntitiesInRange<MindContainerComponent>(center, aura.Range))
            {
                if (ent.Owner == uid)
                    continue;

                //check if entity is a player
                if (!HasComp<MindContainerComponent>(ent))
                    continue;

                nowInside.Add(ent.Owner);

                if (!aura.Accumulated.ContainsKey(ent.Owner))
                    aura.Accumulated[ent.Owner] = 0f;

                aura.Accumulated[ent.Owner] += frameTime;

                if (!(aura.StartingEffects == null))
                {
                    if (_entityManager.TryGetComponent(ent.Owner, out MindContainerComponent? mind) && mind.HasMind)
                    {
                        HandleThresholds(uid, ent.Owner, aura.Accumulated[ent.Owner], aura);
                    }
                }
            }

            // Handle removals (reset timer if they leave range)
            var toRemove = new List<EntityUid>();

            foreach (var tracked in aura.Accumulated.Keys)
            {
                if (!nowInside.Contains(tracked))
                    toRemove.Add(tracked);
            }

            foreach (var rem in toRemove)
            {
                aura.Accumulated.Remove(rem);
                aura.FiredEffects.Remove(rem);
            }
        }
    }


    private void HandleThresholds(EntityUid source, EntityUid target, float time, AccumulationAuraComponent aura)
    {
        foreach (var effect in aura.StartingEffects)
        {
            if (time >= effect.Value)
            {
                if (aura.FiredEffects.ContainsKey(target))
                {
                    if (!aura.FiredEffects[target].Contains(effect.Key))
                    {
                        //shit that applies status effects
                        _statusEffects.TryAddStatusEffect(target, effect.Key, out var status);
                        _chat.TryEmoteWithChat(target, "Scream");
                        aura.FiredEffects[target].Add(effect.Key);
                    }
                }
                else
                {
                    aura.FiredEffects[target] = [effect.Key];
                    //shit that applies status effect
                    _chat.TryEmoteWithChat(target, "Scream");
                    _statusEffects.TryAddStatusEffect(target, effect.Key, out var status);
                }
            }
        }
    }
}