using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace DarkSoulsParry
{
    public enum ParrySound
    {
        // Demon Souls parry SFX is same as DS1
        DS1,
        DS2,
        Bloodborne,
        // Dark Souls 3 parry SFX is same as DS1 & Sekiro Deathblow
        // Dark Souls Remastered parry SFX is same as DS1
        Sekiro,
        SekiroHuge,
        DeSR,
        EldenRing
    }

    public enum ParryType
    {
        Slowed,
        Stagger,
        Posture,
        Tiered
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

        public static ModOptionFloat[] zeroToEightWith2Tenths()
        {
            ModOptionFloat[] options = new ModOptionFloat[41];
            float val = 0;
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new ModOptionFloat(val.ToString("0.0"), val);
                val += 0.2f;
            }
            return options;
        }

        public static ModOptionInt[] oneToTen()
        {
            ModOptionInt[] options = new ModOptionInt[10];
            int val = 1;
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new ModOptionInt(val.ToString("0"), val);
                val++;
            }
            return options;
        }

        // +-------------+
        // |   GENERAL   |
        // +-------------+ 

        [ModOption(name: "Use FromSoftware Parries", tooltip: "Turns on/off the FromSoftware parries mod.", defaultValueIndex = 1, order = 0)]
        public static bool useFSParries;

        [ModOption(name: "Parry Type", tooltip: "Determines what type parry to perform.", defaultValueIndex = 0, order = 1)]
        public static ParryType parryType;

        [ModOption(name: "Shield Only Parries", tooltip: "Determines if parries should only occur when the parry item is a shield.", defaultValueIndex = 0, order = 2)]
        public bool shieldOnlyParries;

        // +--------------+
        // |    SLOWED    |
        // +--------------+ 

        [ModOption(name: "Slowed Parry Min. Velocity", tooltip: "Determines the minimum velocity weapons must clash at to register as a Dark Souls 1 parry.", valueSourceName = nameof(zeroToEightWith2Tenths), defaultValueIndex = 30, category = "Slowed (DS1, BB, ER)", order = 0)]
        public static float slowedMinVelocity;

        [ModOption(name: "Slow Duration", tooltip: "Determines the duration a creature is slowed for after a Dark Souls 1 parry.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 4, category = "Slowed (DS1, BB, ER)", order = 1)]
        public static float slowedParryDuration;

        [ModOption(name: "Slow Percentage", tooltip: "Determines a creature's speed on parry in percentage. This number should be low to notice a difference.", valueSourceName = nameof(zeroToOneHundered), defaultValueIndex = 10, category = "Slowed (DS1, BB, ER)", order = 2)]
        public static float slowedParrySlow;

        [ModOption(name: "Slowed Parry SFX", tooltip: "Determines the parry sound that will play for a Dark Souls 1 parry.", defaultValueIndex = 0, category = "Slowed (DS1, BB, ER)", order = 3)]
        public static ParrySound slowedParrySound;

        // +-------------+
        // |   STAGGER   |
        // +-------------+ 

        [ModOption(name: "Stagger Parry Min. Velocity", tooltip: "Determines the minimum velocity weapons must clash at to register as a Dark Souls 2 parry.", valueSourceName = nameof(zeroToEightWith2Tenths), defaultValueIndex = 30, category = "Stagger (DS2, DeSR)", order = 0)]
        public static float staggerMinVelocity;

        [ModOption(name: "Stagger Delay", tooltip: "Determines how long to wait to destabilize the creature after a Dark Souls 2 parry.", valueSourceName = nameof(zeroToHundredWithTenths), defaultValueIndex = 3, category = "Stagger (DS2, DeSR)", order = 1)]
        public static float staggerDelayDuration;

        [ModOption(name: "Stagger Parry SFX", tooltip: "Determines the parry sound that will play for a Dark Souls 2 parry.", defaultValueIndex = 1, category = "Stagger (DS2, DeSR)", order = 2)]
        public static ParrySound staggerParrySound;

        // +-------------+
        // |   POSTURE   |
        // +-------------+ 

        [ModOption(name: "Posture Min. Velocity", tooltip: "Determines the minimum velocity weapons must clash at to register as a Sekiro parry.", valueSourceName = nameof(zeroToEightWith2Tenths), defaultValueIndex = 30, category = "Posture", order = 0)]
        public static float postureMinVelocity;

        [ModOption(name: "Posture Parry Count", tooltip: "Determines how many parries need to be done to stagger an enemy.", valueSourceName = nameof(oneToTen), defaultValueIndex = 2, category = "Posture (Sekiro)", order = 1)]
        public static int postureParryCount;
        private Dictionary<Creature, int> allCreaturePoise = new Dictionary<Creature, int>();

        [ModOption(name: "Use Posture Parry Sound", tooltip: "Determines whether to play a SFX on a posture parry (NOT on a stagger).", defaultValueIndex = 1, category = "Posture (Sekiro)", order = 2)]
        public static bool postureUseClashSFX;

        [ModOption(name: "Posture Parry SFX", tooltip: "Determines the parry sound that will play on a stagger for Sekiro parries.", defaultValueIndex = 3, category = "Posture (Sekiro)", order = 3)]
        public static ParrySound postureParrySound;

        // +--------------+
        // |    TIERED    |
        // +--------------+
 
        [ModOption(name: "Minimum Tier 1 Velocity", tooltip: "Determines the minimum velocity weapons must clash at to register as a Tier 1 parry.", valueSourceName = nameof(zeroToEightWith2Tenths), defaultValueIndex = 30, category = "Tiered", order = 0)]
        public static float tier1MinParry;

        [ModOption(name: "Minimum Tier 2 Velocity", tooltip: "Determines the minimum velocity weapons must clash at to register as a Tier 2 parry.", valueSourceName = nameof(zeroToEightWith2Tenths), defaultValueIndex = 40, category = "Tiered", order = 0)]
        public static float tier2MinParry;

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
            EventManager.onCreatureKill += OnCreatureKill;
            EventManager.onCreatureParry += EventManager_onCreatureParry;
        }

        private void EventManager_onCreatureParry(Creature creature, CollisionInstance collisionInstance)
        {
            if (!useFSParries)
                return;

            // if valid parry item
            if (collisionInstance.targetCollider.GetComponentInParent<Item>() != null)
            {
                Item item = collisionInstance.targetCollider.GetComponentInParent<Item>();
                // ... and held by the player
                if (Player.currentCreature.equipment.GetHeldItem(Side.Right) == item
                    || Player.currentCreature.equipment.GetHeldItem(Side.Left) == item)
                {
                    if (!shieldOnlyParries || (shieldOnlyParries && item.data.type == ItemData.Type.Shield))
                    {
                        float velocity = item.physicBody.velocity.magnitude;
                        switch (parryType)
                        {
                            case ParryType.Slowed:
                                if (velocity >= slowedMinVelocity)
                                    GameManager.local.StartCoroutine(SlowedParry(creature));
                                break;
                            case ParryType.Stagger:
                                if (velocity >= staggerMinVelocity)
                                    GameManager.local.StartCoroutine(StaggerParry(creature));
                                break;
                            case ParryType.Posture:
                                if (velocity >= postureMinVelocity)
                                    PostureParry(creature, collisionInstance);
                                break;
                            case ParryType.Tiered:
                                if (velocity >= tier2MinParry)
                                    GameManager.local.StartCoroutine(SlowedParry(creature));
                                else if (velocity >= tier1MinParry)
                                    GameManager.local.StartCoroutine(StaggerParry(creature));
                                break;
                        }
                    }
                }
            }
        }

        private void OnCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart && creature.animator.speed == slowedParrySlow)
                creature.animator.speed = 1.0f;

            if (allCreaturePoise.ContainsKey(creature))
                allCreaturePoise.Remove(creature);
        }

        private void PostureParry(Creature creature, CollisionInstance collisionInstance)
        {
            System.Random random = new System.Random();

            if (!allCreaturePoise.ContainsKey(creature))
                allCreaturePoise.Add(creature, postureParryCount);

            allCreaturePoise[creature]--;

            if (allCreaturePoise[creature] == 0)
            {
                if (postureParrySound == ParrySound.Sekiro)
                    Catalog.GetData<EffectData>("SekiroBigParry").Spawn(collisionInstance.contactPoint, Quaternion.identity).Play();

                Catalog.GetData<EffectData>(postureParrySound.ToString()).Spawn(Player.currentCreature.transform).Play();
                creature.TryPush(Creature.PushType.Magic, -creature.brain.transform.forward, 3, 0);
                allCreaturePoise.Remove(creature);
            } else if (postureUseClashSFX)
            {
                Catalog.GetData<EffectData>("SekiroParry").Spawn(collisionInstance.contactPoint, Quaternion.identity).Play();
            }
        }

        private IEnumerator SlowedParry(Creature creature)
        {
            Catalog.GetData<EffectData>(slowedParrySound.ToString()).Spawn(Player.currentCreature.transform).Play();
            creature.animator.speed = slowedParrySlow / 100;
            yield return new WaitForSeconds(slowedParryDuration);
            creature.animator.speed = 1.0f;
        }

        private IEnumerator StaggerParry(Creature creature)
        {
            Catalog.GetData<EffectData>(staggerParrySound.ToString()).Spawn(Player.currentCreature.transform).Play();
            yield return new WaitForSeconds(staggerDelayDuration);
            creature.TryPush(Creature.PushType.Magic, -creature.brain.transform.forward, 3, 0);
        }
    }
}
