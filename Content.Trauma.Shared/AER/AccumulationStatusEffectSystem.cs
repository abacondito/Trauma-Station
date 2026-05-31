using Content.Shared.Bed.Sleep;
using Content.Shared.Drowsiness;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.Aer;

public sealed partial class AccumulationStatusEffectSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<AccumulationStatusEffectComponent, StatusEffectAppliedEvent>(OnEffectApplied);
    }

    private void OnEffectApplied(Entity<AccumulationStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        ent.Comp.TimeOfApplication = _timing.CurTime;
        ent.Comp.FiredEffects.Clear();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AccumulationStatusEffectComponent>();
        while (query.MoveNext(out var target, out var accumulationStatusEffect))
        {
            HandleThresholds(target, accumulationStatusEffect, _timing.CurTime);

        }
    }

    private void HandleThresholds(EntityUid target, AccumulationStatusEffectComponent accumulation, TimeSpan curTime)
    {
        foreach (var effect in accumulation.StartingEffects)
        {
            if (curTime >= accumulation.TimeOfApplication + TimeSpan.FromSeconds(effect.Key))
            {
                if (!accumulation.FiredEffects.Contains(effect.Value))
                {
                    _effects.ApplyEffect(target, effect.Value);
                    accumulation.FiredEffects.Add(effect.Value);
                }
            }
        }
    }

}
