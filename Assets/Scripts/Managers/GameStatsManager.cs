using Singleton;
using UnityEngine;

namespace Managers
{
    public class GameStatsManager : SingletonBase<GameStatsManager>
    {
        [Header("Tracked Stats")]
        public float moneyEarnedToDate;
        public int totalKills;
        public int upgradesPurchased;
        public int daysSurvived;
        public int oreMined;

        protected override void Awake()
        {
            persistBetweenScenes = true;
            base.Awake();
        }

        public void AddMoneyEarned(float amount)
        {
            if (amount > 0)
            {
                moneyEarnedToDate += amount;
            }
        }

        public void IncrementKills()
        {
            totalKills++;
        }

        public void IncrementUpgradesPurchased()
        {
            upgradesPurchased++;
        }

        public void IncrementDaysSurvived()
        {
            daysSurvived++;
        }

        public void IncrementOreMined()
        {
            oreMined++;
        }
    }
}
