using System;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Map;
using UnityEngine;
using MetaProgressData = CodeBase.Data.MetaProgress;

namespace CodeBase.Infrastructure.Services.MetaProgress
{
    public class MetaProgressService : IMetaProgressService
    {
        private const string PrefsKey = "MetaProgress";

        private readonly IStaticDataService _staticData;
        private readonly IPersistentProgressService _persistentProgress;

        private MetaProgressData _data = new MetaProgressData();

        public event Action Changed;

        public MetaProgressService(IStaticDataService staticData, IPersistentProgressService persistentProgress)
        {
            _staticData = staticData;
            _persistentProgress = persistentProgress;
        }

        public int Essence => _data.essence;
        public int RunsFinished => _data.runsFinished;
        public int RunsWon => _data.runsWon;
        public int BestNodesCompleted => _data.bestNodesCompleted;

        public void Load()
        {
            string json = PlayerPrefs.GetString(PrefsKey, string.Empty);

            MetaProgressData loaded = null;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    loaded = JsonUtility.FromJson<MetaProgressData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Не вдалося прочитати MetaProgress, починаємо з чистого: {e.Message}");
                }
            }

            _data = loaded ?? new MetaProgressData();
            if (_data.unlockedHeroes == null)
                _data.unlockedHeroes = new System.Collections.Generic.List<HeroTypeId>();
        }

        public void Save()
        {
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(_data));
            PlayerPrefs.Save();
        }

        public bool IsHeroUnlocked(HeroStaticData hero)
        {
            if (hero == null) return false;
            return hero.unlockCost <= 0 || _data.unlockedHeroes.Contains(hero.heroTypeId);
        }

        public bool CanUnlockHero(HeroStaticData hero) =>
            hero != null && !IsHeroUnlocked(hero) && _data.essence >= hero.unlockCost;

        public bool TryUnlockHero(HeroStaticData hero)
        {
            if (hero == null) return false;
            if (IsHeroUnlocked(hero)) return true;
            if (_data.essence < hero.unlockCost) return false;

            _data.essence -= hero.unlockCost;
            _data.unlockedHeroes.Add(hero.heroTypeId);

            Save();
            Changed?.Invoke();
            return true;
        }

        public int CompleteRun(bool won)
        {
            int nodesCompleted = NodesCompletedThisRun();
            int reward = CalculateReward(nodesCompleted, won);

            _data.essence += reward;
            _data.runsFinished++;
            if (won)
                _data.runsWon++;
            if (nodesCompleted > _data.bestNodesCompleted)
                _data.bestNodesCompleted = nodesCompleted;

            Save();
            Changed?.Invoke();
            return reward;
        }

        private int NodesCompletedThisRun()
        {
            Data.PlayerProgress progress = _persistentProgress.Progress;
            if (progress == null || progress.mapProgress == null || progress.mapProgress.completedNodeIds == null)
                return 0;

            return progress.mapProgress.completedNodeIds.Count;
        }

        private int CalculateReward(int nodesCompleted, bool won)
        {
            MapStaticData settings = _staticData.GetMapSettings();
            if (settings == null) return 0;

            int reward = nodesCompleted * settings.essencePerCompletedNode;
            if (won)
                reward += settings.essenceForWin;

            return Mathf.Max(0, reward);
        }
    }
}
