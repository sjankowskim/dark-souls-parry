using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace DarkSoulsParry
{
    public enum ParrySound
    {
        DS1,
        DS2,
        EldenRing
    }

    public class DarkSoulsParry : ThunderScript
    {
        public static ModOptionFloat[] zeroToOneHundered()
        {
            ModOptionFloat[] options = new ModOptionFloat[101];
            float val = 0;
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new ModOptionFloat(val.ToString("0.0"), val);
                val += 1f;
            }
            return options;
        }

        public static ModOptionFloat[] zeroToHundredWithTenths()
        {
            ModOptionFloat[] options = new ModOptionFloat[1001];
            float val = 0;
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new ModOptionFloat(val.ToString("0.0"), val);
                val += 0.1f;
            }
            return options;
        }

        [ModOption(name: "Use Dark Souls Parry Mod", tooltip: "Turns on/off the Dark Souls Parry mod.", defaultValueIndex = 1, order = 0)]
        public static bool useDSParries;

        [ModOption(name: "Minimum Weapon Velocity", tooltip: "Determines the minimum velocity weapons must clash at in order to register as a Dark Souls parry.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 6, order = 1)]
        public static float minWeaponVelocity;

        [ModOption(name: "Shield Only Parries", tooltip: "Determines if parries should only occur when the parry item is a shield.", defaultValueIndex = 0, order = 2)]
        public bool shieldOnlyParries;

        [ModOption(name: "Parry Duration", tooltip: "Determines the duration a creature is slowed for after a Dark Souls 1 parry.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 4, category = "Dark Souls 1", order = 0)]
        public static float ds1ParryDuration;

        [ModOption(name: "Slow Percentage", tooltip: "Determines how slow a creature will be after a Dark Souls 1 parry.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 10, category = "Dark Souls 1", order = 1)]
        public static float ds1ParrySlow;

        [ModOption(name: "Parry Sound", tooltip: "Determines the parry sound that will play for a Dark Souls 1 parry.", defaultValueIndex = 0, category = "Dark Souls 1", order = 2)]
        public static ParrySound ds1ParrySound;

        [ModOption(name: "Use Dark Souls 2 Parry", tooltip: "Determines if the player wants to use Dark Souls 2 parries instead.", defaultValueIndex = 0, category = "Dark Souls 2", order = 0)]
        public static bool useDarkSouls2Parry;

        [ModOption(name: "Delay Duration", tooltip: "Determines how long to wait after a Dark Souls 2 parry to destabilize the creature.", valueSourceName = nameof(zeroToHundredWithTenths), defaultValueIndex = 3, category = "Dark Souls 2", order = 1)]
        public static float ds2DelayDuration;

        [ModOption(name: "Parry Sound", tooltip: "Determines the parry sound that will play for a Dark Souls 2 parry.", defaultValueIndex = 1, category = "Dark Souls 2", order = 2)]
        public static ParrySound ds2ParrySound;

        [ModOption(name: "Use Tiered Parry", tooltip: "Determines if the player wants to use a tiered parry, where exceeding minWeaponVelocity will do a Dark Souls 1 parry, and exceeding minTier2Velocity will do a Dark Souls 2 parry.", defaultValueIndex = 0, category = "Tiered Parry", order = 0)]
        public static bool useTieredParry;

        [ModOption(name: "Minimum Weapon Velocity", tooltip: "Determines the minimum velocity weapons must clash at in order to register as a Dark Souls 2 parry for the tiered parry.", valueSourceName = nameof(zeroToHundredWithTenths), defaultValueIndex = 70, category = "Tiered Parry", order = 1)]
        public static float minTier2Velocity;

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
            EventManager.onCreatureParry += OnCreatureParry;
            EventManager.onCreatureKill += OnCreatureKill;
        }

        private void OnCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
                if (creature.animator.speed == ds1ParrySlow)
                    creature.animator.speed = 1.0f;
        }

        private void OnCreatureParry(Creature creature, CollisionInstance collisionInstance)
        {
            if (useDSParries)
            {
                // if valid parry item
                if (collisionInstance.targetCollider.GetComponentInParent<Item>() != null)
                {
                    Item item = collisionInstance.targetCollider.GetComponentInParent<Item>();
                    // ... and held by the player
                    if (Player.currentCreature.equipment.GetHeldItem(Side.Right) == item
                        || Player.currentCreature.equipment.GetHeldItem(Side.Left) == item)
                    {
                        // ... and hit with high enough velocity
                        if (item.physicBody.velocity.magnitude >= minWeaponVelocity)
                        {
                            if (!shieldOnlyParries || (shieldOnlyParries && item.data.type == ItemData.Type.Shield))
                                GameManager.local.StartCoroutine(ParryCoroutine(creature, item.physicBody.velocity.magnitude));
                        }
                    }
                }
            }
        }

        private IEnumerator ParryCoroutine(Creature creature, float velocity)
        {
            if (useTieredParry)
            {
                if (velocity >= minTier2Velocity)
                    GameManager.local.StartCoroutine(DarkSouls2Parry(creature));
                else
                    GameManager.local.StartCoroutine(DarkSouls1Parry(creature));
            }
            else
            {
                if (useDarkSouls2Parry)
                    GameManager.local.StartCoroutine(DarkSouls2Parry(creature));
                else
                    GameManager.local.StartCoroutine(DarkSouls1Parry(creature));
            }
            yield return null;
        }

        private IEnumerator DarkSouls1Parry(Creature creature)
        {
            Catalog.GetData<EffectData>(ds1ParrySound.ToString()).Spawn(Player.currentCreature.transform).Play();
            creature.animator.speed = ds1ParrySlow / 100;
            creature.locomotion.rb.velocity = Vector3.zero;
            yield return new WaitForSeconds(ds1ParryDuration);
            creature.animator.speed = 1.0f;
        }

        private IEnumerator DarkSouls2Parry(Creature creature)
        {
            Catalog.GetData<EffectData>(ds2ParrySound.ToString()).Spawn(Player.currentCreature.transform).Play();
            yield return new WaitForSeconds(ds2DelayDuration);
            creature.TryPush(Creature.PushType.Magic, -creature.brain.transform.forward, 3, 0);
        }
    }
}
