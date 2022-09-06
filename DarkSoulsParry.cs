using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace DarkSoulsParry
{
    public class DarkSoulsParry : LevelModule
    {
        public bool useDSParries = true;
        public float minWeaponVelocity = 6.0f;
        public float ds1ParryDuration = 4.0f;
        public float ds1ParrySlow = 0.10f;
        public bool useDarkSouls2Parry = false;
        public float ds2DelayDuration = 0.3f;
        public bool useTieredParry = false;
        public float minTier2Velocity = 6.0f;

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
            Catalog.GetData<EffectData>("DS1Parry").Spawn(Player.currentCreature.transform).Play();
            creature.animator.speed = ds1ParrySlow;
            yield return new WaitForSeconds(ds1ParryDuration);
            creature.animator.speed = 1.0f;
        }

        private IEnumerator DarkSouls2Parry(Creature creature)
        {
            Catalog.GetData<EffectData>("DS2Parry").Spawn(Player.currentCreature.transform).Play();
            yield return new WaitForSeconds(ds2DelayDuration);
            creature.TryPush(Creature.PushType.Magic, -creature.brain.transform.forward, 3, 0);
        }
    }
}
