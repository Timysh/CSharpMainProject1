using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";

        // Константы для перегрева
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;

        // Переменные для перегрева
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        // Список целей, к которым идем
        private List<Vector2Int> _targetsToGoTo = new List<Vector2Int>();

     
        //Счетчик для выдачи номеров. static, чтобы быть общим для всех юнитов
        private static int _unitCounter = 0;

        //этоуникальный номер этого конкретного юнита
        private int _unitNumber;

        //максимум целей среди которых распределяем юнитов
        private const int MaxTargetsToConsider = 3;

        //конструктор,вызывается при создании каждого нового юнита
        public SecondUnitBrain()
        {
            //Выдаем текущий номер
            _unitNumber = _unitCounter;
            //Увеличиваем счетчик для следующего юнита
            _unitCounter = _unitCounter + 1;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            //Узнаём текущую температуру
            int currentTemp = GetTemperature();

            //Если температуар слишком большая, то не стреляем
            if (currentTemp >= overheatTemperature)
            {
                return;
            }
            else
            {
                //Иначе нагреваем оружие на 1
                IncreaseTemperature();
            }

            //Узнаём новую температуру
            int newTemp = GetTemperature();

            //Количество снарядов = текущая температура
            int shotsCount = newTemp;

            //Создаём нужное количество снарядов
            for (int i = 0; i < shotsCount; i++)
            {
                BaseProjectile projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            //Если список целей пустой — стоим на месте
            if (_targetsToGoTo.Count == 0)
            {
                return unit.Pos;
            }

            //Берём первую цель из списка (после нового алгоритма там лежит только наша цель)
            Vector2Int target = _targetsToGoTo[0];

            //Проверяем нахождение цели в зоне атаки
            bool targetInRange = IsTargetInRange(target);

            if (targetInRange == true)
            {
                //Цель рядом, не двигаемся и атакуем
                return unit.Pos;
            }
            else
            {
                //Цель далеко, то делаем один шаг в её сторону
                Vector2Int nextPosition = unit.Pos.CalcNextStepTowards(target);
                return nextPosition;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            //Создаём пустой список для результата
            List<Vector2Int> result = new List<Vector2Int>();

            //Очищаем список целей, к которым надо идти
            _targetsToGoTo.Clear();

            //Получаем все цели
            IEnumerable<Vector2Int> allTargetsEnumerable = GetAllTargets();

            //Добавляем их заново в очищенный список
            foreach (Vector2Int t in allTargetsEnumerable)
            {
                _targetsToGoTo.Add(t);
            }

            //Если в списке целей никого нет, то добавляем в список базу противника
            if (_targetsToGoTo.Count == 0)
            {
                int enemyId;
                if (IsPlayerUnitBrain == true)
                {
                    enemyId = RuntimeModel.BotPlayerId;
                }
                else
                {
                    enemyId = RuntimeModel.PlayerId;
                }

                Vector2Int enemyBasePosition = runtimeModel.RoMap.Bases[enemyId];
                _targetsToGoTo.Add(enemyBasePosition);
            }

            //Производим сортировку целей по дистанции до нашей базы
            SortByDistanceToOwnBase(_targetsToGoTo);

            //Узнаем, сколько всего целей есть
            int targetsCount = _targetsToGoTo.Count;

            //Определяем, сколько целей мы вообще будем рассматривать. Три штуки, это максимаьлное
            int targetsToConsider = MaxTargetsToConsider;
            if (targetsCount < targetsToConsider)
            {
                //Если целей меньше 3, рассматриваем столько, сколько есть
                targetsToConsider = targetsCount;
            }

            //Вычисляем индекс цели для этого юнита с помощью остатка от деления
            int targetIndex = _unitNumber % targetsToConsider;

            // Берем цель под нужным номером
            Vector2Int chosenTarget = _targetsToGoTo[targetIndex];

            //Очищаем список, так как для движения нам нужна только ОДНА выбранная цель
            _targetsToGoTo.Clear();
            _targetsToGoTo.Add(chosenTarget);

            //Проверяем, в зоне ли атаки эта цель
            bool inRange = IsTargetInRange(chosenTarget);

            if (inRange == true)
            {
                //Если в зоне атаки, то добавляем в результат и будем стрелять
                result.Add(chosenTarget);
            }

            // Возвращаем список целей для атаки
            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown / 10);
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
            if (_overheated)
            {
                return (int)OverheatTemperature;
            }
            else
            {
                return (int)_temperature;
            }
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;

            if (_temperature >= OverheatTemperature)
            {
                _overheated = true;
            }
        }
    }
}
