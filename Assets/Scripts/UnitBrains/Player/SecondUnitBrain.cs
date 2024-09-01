using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> unreachableTargets;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////

            int Temp = GetTemperature();
            if (Temp >= overheatTemperature)
            {
                return;
            }
            IncreaseTemperature();
            for (int i = 0; i <= Temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            ///////////////////////////////////////           
        }

        public override Vector2Int GetNextStep()
        {
            List<Vector2Int> targets = SelectTargets();

            Vector2Int unitPos = unit.Pos;
            Vector2Int nextTarget = new Vector2Int(0, 0);
            if (targets.Count > 0)
            {
                foreach (var target in targets)
                {
                    if (IsTargetInRange(target))
                    {
                        return unit.Pos;
                    }
                    else
                    {
                        nextTarget = target;
                    }
                }
            }
            else
            {
                int playerId = IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[playerId];
                if (IsTargetInRange(enemyBase))
                {
                    return unit.Pos;
                }
                else
                {
                    unreachableTargets.Add(enemyBase);
                    nextTarget = enemyBase;
                }
            }

            return unitPos.CalcNextStepTowards(nextTarget);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            List<Vector2Int> result = GetReachableTargets();

            if (result.Count == 0)
            {
                return new List<Vector2Int>();
            }

            Vector2Int Target = new Vector2Int();
            float minDistance = float.MaxValue;
            unreachableTargets = new List<Vector2Int>();
            foreach (Vector2Int TargetEnemy in result)
            {
                float Distance = DistanceToOwnBase(TargetEnemy);
                if (Distance < minDistance)
                {
                    minDistance = Distance;
                    Target = TargetEnemy;
                }
            }

            result.Clear();
            result.Add(Target);

            while (result.Count > 1)
            {
                result.RemoveAt(result.Count - 1);
            }
            return result;
            ///////////////////////////////////////
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}