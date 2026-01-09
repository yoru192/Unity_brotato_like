using System.Collections.Generic;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.Services.Upgrade
{
    public interface IUpgradeService : IService
    {
        List<UpgradeStaticData> GenerateUpgradeOptions(int count = 3);
        void ApplyUpgrade(UpgradeStaticData upgrade);
    }
}