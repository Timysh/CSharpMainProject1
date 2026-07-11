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
            ///


            //Получаем список всех врагов, в которых можем стрелять
            List<Vector2Int> result = GetReachableTargets();

            //Если врагов нет – сразу возвращаем пустой список
            if (result.Count == 0)
            {
                return result;
            }

            //Превращаем список в массив
            Vector2Int[] targetsArray = result.ToArray();

            //Создаём массив для расстояний от каждой цели до нашей базы
            float[] distances = new float[targetsArray.Length];

            //Заполняем массив расстояний
            for (int i = 0; i < targetsArray.Length; i++)
            {
                Vector2Int currentTarget = targetsArray[i];
                float distanceToBase = DistanceToOwnBase(currentTarget);
                distances[i] = distanceToBase;
            }

            //Ищем наименьшее расстояние в массиве
            float minDistance = float.MaxValue;
            int indexOfMin = 0;

            //Проходим по всем элементам массива расстояний
            for (int i = 0; i < distances.Length; i++)
            {
                // Если текущее расстояние меньше, чем уже найденное минимальное,
                // то обновляем минимум и запоминаем индекс
                if (distances[i] < minDistance)
                {
                    minDistance = distances[i];
                    indexOfMin = i;
                }
            }

            //Теперь мы знаем индекс цели, которая находится ближе всех к базе
            //Берём эту цель из массива targetsArray
            Vector2Int closestTarget = targetsArray[indexOfMin];

            //Очищаем список result и добавляем только одну цель.
            result.Clear();
            result.Add(closestTarget);

            //Возвращаем результат – список с одной целью.
            return result;
            
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