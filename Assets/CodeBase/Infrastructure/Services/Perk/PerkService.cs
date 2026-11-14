using System;
using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData.Perk;

namespace CodeBase.Infrastructure.Services.Perk
{
    public class PerkService : IPerkService
    {
        private readonly IPersistentProgressService _progressService;

        public event Action Changed;

        public PerkService(IPersistentProgressService progressService)
        {
            _progressService = progressService;
        }

        // Прогрес створюється заново на кожен забіг, тому список беремо щоразу, а не кешуємо.
        private List<PerkEntry> Perks => _progressService.Progress?.playerState?.perks;

        public float GetValue(PerkId id)
        {
            int index = IndexOf(id);
            return index >= 0 ? Perks[index].value : 0f;
        }

        public int GetPicks(PerkId id)
        {
            int index = IndexOf(id);
            return index >= 0 ? Perks[index].picks : 0;
        }

        public bool Has(PerkId id) => IndexOf(id) >= 0;

        public void Add(PerkId id, float value)
        {
            if (id == PerkId.None) return;

            List<PerkEntry> perks = Perks;
            if (perks == null) return;

            int index = IndexOf(id);
            if (index >= 0)
            {
                PerkEntry entry = perks[index];
                entry.value += value;
                entry.picks++;
                perks[index] = entry;
            }
            else
            {
                perks.Add(new PerkEntry { id = id, value = value, picks = 1 });
            }

            Changed?.Invoke();
        }

        private int IndexOf(PerkId id)
        {
            List<PerkEntry> perks = Perks;
            if (perks == null) return -1;

            for (int i = 0; i < perks.Count; i++)
            {
                if (perks[i].id == id)
                    return i;
            }

            return -1;
        }
    }
}
