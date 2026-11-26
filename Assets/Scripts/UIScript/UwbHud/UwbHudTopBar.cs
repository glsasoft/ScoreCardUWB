using TMPro;
using UnityEngine;

namespace LetsPiu.UI
{
    /// <summary>
    /// 顶部比分条：红蓝积分（同时驱动进度条）、剩余时间、两侧旗帜上的总淘汰数，以及比分条下方的任务说明。
    /// 数据读自 UwbHudData，只在变化时才写文字。
    /// </summary>
    public class UwbHudTopBar : MonoBehaviour
    {
        [SerializeField] TMP_Text timerText;
        [SerializeField] TMP_Text redScoreText;
        [SerializeField] TMP_Text blueScoreText;
        [SerializeField] TMP_Text redEliminatedText;
        [SerializeField] TMP_Text blueEliminatedText;
        [SerializeField] TMP_Text taskTitleText;
        [SerializeField] TMP_Text taskBodyText;
        [SerializeField] UwbScoreBar scoreBar;

        int m_Seconds = int.MinValue;
        int m_RedScore = int.MinValue;
        int m_BlueScore = int.MinValue;
        int m_RedEliminated = int.MinValue;
        int m_BlueEliminated = int.MinValue;
        string m_TaskTitle;
        string m_TaskBody;

        public void Refresh()
        {
            int seconds = UwbHudData.RemainSeconds < 0f ? -1 : Mathf.CeilToInt(UwbHudData.RemainSeconds);
            if (m_Seconds != seconds)
            {
                m_Seconds = seconds;
                timerText.text = seconds < 0 ? "--:--" : (seconds / 60).ToString("D2") + ":" + (seconds % 60).ToString("D2");
            }

            bool scoreChanged = false;
            if (m_RedScore != UwbHudData.RedScore)
            {
                m_RedScore = UwbHudData.RedScore;
                redScoreText.text = m_RedScore.ToString();
                scoreChanged = true;
            }

            if (m_BlueScore != UwbHudData.BlueScore)
            {
                m_BlueScore = UwbHudData.BlueScore;
                blueScoreText.text = m_BlueScore.ToString();
                scoreChanged = true;
            }

            if (scoreChanged)
                scoreBar.Apply(m_RedScore, m_BlueScore);

            if (m_RedEliminated != UwbHudData.RedEliminated)
            {
                m_RedEliminated = UwbHudData.RedEliminated;
                redEliminatedText.text = m_RedEliminated.ToString();
            }

            if (m_BlueEliminated != UwbHudData.BlueEliminated)
            {
                m_BlueEliminated = UwbHudData.BlueEliminated;
                blueEliminatedText.text = m_BlueEliminated.ToString();
            }

            if (m_TaskTitle != UwbHudData.TaskTitle)
            {
                m_TaskTitle = UwbHudData.TaskTitle;
                taskTitleText.text = m_TaskTitle;
            }

            if (m_TaskBody != UwbHudData.TaskBody)
            {
                m_TaskBody = UwbHudData.TaskBody;
                taskBodyText.text = m_TaskBody;
            }
        }
    }
}
