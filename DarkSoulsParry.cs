using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using ThunderRoad;
using UnityEngine;

namespace DarkSoulsParry
{
    [Serializable]
    public class ParryOptions
    {
        public bool useDSParries;
        public float minWeaponVelocity;
        public bool useDarkSouls2Parry;
        public bool useTieredParry;
        public float minTier2Velocity;
        public float ds1ParryDuration;
        public float ds2DelayDuration;
        public float ds1ParrySlow;
    }

    public class DarkSoulsParry : LevelModule
    {
        public const string OPTIONS_FILE_PATH = "\\Mods\\DarkSoulsParry\\ParryOptions.opt";
        public static ParryOptions data;

        public override IEnumerator OnLoadCoroutine()
        {
            try
            {
                data = JsonConvert.DeserializeObject<ParryOptions>(File.ReadAllText(Application.streamingAssetsPath + OPTIONS_FILE_PATH));
            }
            catch
            {
                Debug.LogError("Missing ParryOptions.opt. Dark Souls Parry WILL break!");
            }
            EventManager.onCreatureParry += OnCreatureParry;
            EventManager.onCreatureKill += OnCreatureKill;
            return base.OnLoadCoroutine();
        }

        private void OnCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
                if (creature.animator.speed == data.ds1ParrySlow)
                    creature.animator.speed = 1.0f;
        }

        private void OnCreatureParry(Creature creature, CollisionInstance collisionInstance)
        {
            if (data.useDSParries)
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
                        if (item.rb.velocity.magnitude >= data.minWeaponVelocity)
                        {
                            GameManager.local.StartCoroutine(ParryCoroutine(creature, item.rb.velocity.magnitude));
                        }
                    }
                }
            }
        }

        private IEnumerator ParryCoroutine(Creature creature, float velocity)
        {
            if (data.useTieredParry)
            {
                if (velocity >= data.minTier2Velocity)
                    GameManager.local.StartCoroutine(DarkSouls2Parry(creature));
                else
                    GameManager.local.StartCoroutine(DarkSouls1Parry(creature));
            }
            else
            {
                if (data.useDarkSouls2Parry)
                    GameManager.local.StartCoroutine(DarkSouls2Parry(creature));
                else
                    GameManager.local.StartCoroutine(DarkSouls1Parry(creature));
            }
            yield return null;
        }

        private IEnumerator DarkSouls1Parry(Creature creature)
        {
            Catalog.GetData<EffectData>("DS1Parry").Spawn(Player.currentCreature.transform).Play();
            creature.animator.speed = data.ds1ParrySlow;
            yield return new WaitForSeconds(data.ds1ParryDuration);
            creature.animator.speed = 1.0f;
        }

        private IEnumerator DarkSouls2Parry(Creature creature)
        {
            Catalog.GetData<EffectData>("DS2Parry").Spawn(Player.currentCreature.transform).Play();
            yield return new WaitForSeconds(data.ds2DelayDuration);
            creature.TryPush(Creature.PushType.Magic, -creature.brain.transform.forward, 3, 0);
        }
    }
}
