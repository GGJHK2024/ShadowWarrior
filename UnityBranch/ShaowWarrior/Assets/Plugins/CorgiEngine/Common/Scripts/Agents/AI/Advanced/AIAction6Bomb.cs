using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Random = UnityEngine.Random;

namespace MoreMountains.CorgiEngine
{
    public class AIAction6Bomb : AIAction
    {
        protected Character _character;
        protected AIBrain _brain;
        protected Transform _player;
        [SerializeField]
        protected Vector3[] boomPos = new Vector3[6];

        /// <summary>
        /// On init we grab our CharacterHandleWeapon ability
        /// </summary>
        public override void Initialization()
        {
            _character = GetComponentInParent<Character>();
            _character._animator.SetBool("Attack5", true);;
        }

        /// <summary>
        /// 随机炸弹位置
        /// </summary>
        protected void InitBombPos()
        {
            // 玩家位置
            if (LevelManager.HasInstance)
            {
                _player = LevelManager.Instance.Players[0].transform;
                boomPos[0] = _player.position;
            }

            if (_player.position.x < _character.transform.position.x)
            {
                // 玩家在左，左边随两个右边随三个
                boomPos[1] = new Vector3(Random.Range(-17.0f, -1.65f), Random.Range(-4.63f, 1.58f), 0.0f);
                boomPos[2] = new Vector3(Random.Range(-17.0f, -1.65f), Random.Range(-4.63f, 1.58f), 0.0f);
                for (int i = 3; i < 6; i++)
                {
                    boomPos[i] = new Vector3(Random.Range(-1.65f, 13.78f), Random.Range(-4.63f, 1.58f), 0.0f);
                }
            }
            else
            {
                // 玩家在右，左边随三个右边随两个
                boomPos[1] = new Vector3(Random.Range(-1.65f, 13.78f), Random.Range(-4.63f, 1.58f), 0.0f);
                boomPos[2] = new Vector3(Random.Range(-1.65f, 13.78f), Random.Range(-4.63f, 1.58f), 0.0f);
                for (int i = 3; i < 6; i++)
                {
                    boomPos[i] = new Vector3(Random.Range(-17.0f, -1.65f), Random.Range(-4.63f, 1.58f), 0.0f);
                }
            }
        }

        IEnumerator InitBomb()
        {
            foreach (var b in boomPos)
            {
                yield return new WaitForSeconds(0.3f);
                UnityEngine.Object g = Resources.Load<UnityEngine.Object>("Prefabs/BossWeapons/BossWeaponBomb");
                Quaternion r = new Quaternion(0, 0, 0, 0);
                Instantiate(g, b, r);
            }

            yield return null;
        }

        public void Bomb()
        {
            InitBombPos();
            StartCoroutine(InitBomb());
        }


        public override void PerformAction()
        {
            Bomb();
        }
    }
}
