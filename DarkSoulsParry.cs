using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace DarkSoulsParry
{
    public class DarkSoulsParry : LevelModule
    {
        public float minWeaponVelocity = 6.0f;
        public bool useDarkSouls2Parry = false;

        public override IEnumerator OnLoadCoroutine()
        {
            Debug.Log("(Dark Souls Parry) Loaded successfully!");
            EventManager.onCreatureParry += OnCreatureParry;
            return base.OnLoadCoroutine();
        }
            
        private void OnCreatureParry(Creature creature, CollisionInstance collisionInstance)
        {
            // if the weapon used to parry SOMETHING was held by the player
            if (collisionInstance.targetCollider.GetComponentInParent<Item>() != null)
            {
                if (Player.currentCreature.equipment.GetHeldItem(Side.Right) == collisionInstance.targetCollider.GetComponentInParent<Item>()
                    || (Player.currentCreature.equipment.GetHeldItem(Side.Left) == collisionInstance.targetCollider.GetComponentInParent<Item>()))
                {
                    Item playerWeapon = collisionInstance.targetCollider.GetComponentInParent<Item>();
                    if (playerWeapon.rb.velocity.magnitude >= minWeaponVelocity)
                    {
                        EffectData data;
                        if (useDarkSouls2Parry) {
                            data = Catalog.GetData<EffectData>("DS2Parry");
                        } else {
                            data = Catalog.GetData<EffectData>("DS1Parry");
                        }
                        data.Spawn(Player.currentCreature.transform).Play();
                        GameManager.local.StartCoroutine(ParryCoroutine(creature));
                    }
                }
            }
        }

        private IEnumerator ParryCoroutine(Creature parriedCreature)
        {
            if (useDarkSouls2Parry) {
                yield return new WaitForSeconds(0.4f);
                parriedCreature.TryPush(Creature.PushType.Magic, -parriedCreature.brain.transform.forward, 2, 0);
            } else
            {
                parriedCreature.animator.speed = 0.25f;
                yield return new WaitForSeconds(3.0f);
                parriedCreature.animator.speed = 1.0f;
            }
        }
    }
}
