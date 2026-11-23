using CodeBase.Infrastructure.Services.MetaProgress;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Elements
{
    /// <summary>Рядок підсумку забігу на екранах Win / Game Over: пройдені вузли, нарахована й загальна есенція.</summary>
    public class RunSummaryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nodesText;
        [SerializeField] private TextMeshProUGUI _essenceRewardText;
        [SerializeField] private TextMeshProUGUI _essenceTotalText;

        [Tooltip("Позначка \"новий рекорд\" біля кількості вузлів. Можна не призначати.")]
        [SerializeField] private GameObject _newBestBadge;

        public void Show(RunSummary summary)
        {
            _nodesText.text = summary.NodesCompleted.ToString();
            _essenceRewardText.text = $"+{summary.EssenceReward}";
            _essenceTotalText.text = summary.EssenceTotal.ToString();

            if (_newBestBadge != null)
                _newBestBadge.SetActive(summary.IsNewBest);
        }
    }
}
