using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;

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
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////     

            //Узнаём, какая сейчас температура
            int currentTemp = GetTemperature();

            //Если температура уже достигла порога перегрева (3) – стрелять нельзя
            if (currentTemp >= overheatTemperature)
            {
                return; // выход из метода
            }
            else //Иначе производим выстрел: нагреваем оружие на 1 градус
            {
                IncreaseTemperature();
            }

            //Теперь температура выросла. Узнаём новое значение
            int newTemp = GetTemperature();

            //Количество снарядов должно быть равно новой температуре. Например, если стало 2 – выпускаем 2 снаряда
            int shotsCount = newTemp;

            //Создаём и добавляем снаряды в цикле
            for (int i = 0; i < shotsCount; i++)
            {
                // Создаём один снаряд для указанной цели
                BaseProjectile projectile = CreateProjectile(forTarget);
                // Добавляем его в общий список
                AddProjectileToList(projectile, intoList);
            }


            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = GetReachableTargets();
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