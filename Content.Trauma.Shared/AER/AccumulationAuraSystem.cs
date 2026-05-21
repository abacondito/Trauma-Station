using Content.Shared.Mind.Components;
using Content.Shared.Emoting;
using Robust.Shared.Audio.Systems;
using Content.Shared.Chat;
using Content.Shared.StatusEffectNew;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;
using Robust.Shared.Timing;



namespace Content.Trauma.Server.Aer;

public sealed partial class AccumulationAuraSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EmoteSystem _emote = default!;

    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    private HashSet<Entity<MindContainerComponent>> _players = new HashSet<Entity<MindContainerComponent>>();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<AccumulationAuraComponent>();
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

                if (!aura.Accumulated.ContainsKey(ent.Owner))
                    aura.Accumulated[ent.Owner] = curTime;

                if (!(aura.StartingEffects == null))
                {
                    if (TryComp(ent.Owner, out MindContainerComponent? mind) && mind.HasMind)
                    {
                        HandleThresholds(uid, ent.Owner, aura.Accumulated[ent.Owner], curTime, aura);
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


    private void HandleThresholds(EntityUid source, EntityUid target, TimeSpan timeEntered, TimeSpan curTime, AccumulationAuraComponent aura)
    {
        foreach (var effect in aura.StartingEffects)
        {
            if (curTime >= timeEntered + TimeSpan.FromSeconds(effect.Value))
            {
                if (aura.FiredEffects.ContainsKey(target))
                {
                    if (!aura.FiredEffects[target].Contains(effect.Key))
                    {
                        //shit that applies status effects
                        if (!_statusEffects.HasStatusEffect(target, effect.Key))
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
                    if (!_statusEffects.HasStatusEffect(target, effect.Key))
                        _statusEffects.TryAddStatusEffect(target, effect.Key, out var status);
                }
            }
        }
    }
}