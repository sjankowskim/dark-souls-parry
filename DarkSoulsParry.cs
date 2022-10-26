﻿using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace DarkSoulsParry
{
    public enum ParrySound
    {
        DS1Parry,
        DS2Parry,
        EldenRingParry
    }

    public class DarkSoulsParry : LevelModule
    {
        [Tooltip("Turns on/off the Dark Souls Parry mod.")]
        public bool useDSParries = true;
        [Tooltip("Determines the minimum velocity weapons must clash at in order to register as a Dark Souls parry.")]
        [Range(0, 15)]
        public float minWeaponVelocity = 6.0f;
        [Tooltip("Determines the duration a creature is slowed for after a Dark Souls 1 parry.")]
        [Range(0, 100)]
        public float ds1ParryDuration = 4.0f;
        [Tooltip("Determines how slow a creature will be after a Dark Souls 1 parry. This value will be divded by 100. (i.e. 10s -> 0.1s)")]
        [Range(0, 100)]
        public float ds1ParrySlow = 10.0f;
        [Tooltip("Determines the parry sound that will play for a Dark Souls 1 parry.")]
        public ParrySound ds1ParrySound;
        [Tooltip("Determines if the player wants to use Dark Souls 2 parries instead.")]
        public bool useDarkSouls2Parry = false;
        [Tooltip("Determines how long to wait after a Dark Souls 2 parry to destabilize the creature.")]
        [Range(0, 100)]
        public float ds2DelayDuration = 0.3f;
        [Tooltip("Determines the parry sound that will play for a Dark Souls 2 parry.")]
        public ParrySound ds2ParrySound;
        [Tooltip("Determines if the player wants to use a tiered parry, where exceeding minWeaponVelocity will do a Dark Souls 1 parry, and exceeding minTier2Velocity will do a Dark Souls 2 parry.")]
        public bool useTieredParry = false;
        [Tooltip("Determines the minimum velocity weapons must clash at in order to register as a Dark Souls 2 parry for the tiered parry.")]
        [Range(0, 15)]
        public float minTier2Velocity = 7.0f;
        [Tooltip("Determines if parries should only occur when the parry item is a shield.")]
        public bool shieldOnlyParries = false;

        public override IEnumerator OnLoadCoroutine()
        {
            EventManager.onCreatureParry += OnCreatureParry;
            EventManager.onCreatureKill += OnCreatureKill;
            return base.OnLoadCoroutine();
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
                        if (item.rb.velocity.magnitude >= minWeaponVelocity)
                        {
                            if (!shieldOnlyParries || (shieldOnlyParries && item.data.type == ItemData.Type.Shield))
                                GameManager.local.StartCoroutine(ParryCoroutine(creature, item.rb.velocity.magnitude));
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
