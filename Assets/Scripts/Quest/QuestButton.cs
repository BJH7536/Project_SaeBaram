using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class QuestButton : MonoBehaviour
{
    [Tab("QuestInfo")]
    [SerializeField] public TextMeshProUGUI QuestName;
    [SerializeField] public QuestData questData;
    [SerializeField] public Image QuestCheck;
    [Tab("Panel")]
    [SerializeField] public QuestInfoPanel QuestInfoPanel;

    private void Awake()
    {
        this.GetComponent<Button>().onClick.AddListener(() => DisplayQuestInfo());
    }

    public void SetQuestInfo(QuestData data)
    {
        QuestName.text = data.questName;
        questData = data;
    }

    public void SetQuestCheck(QuestData data)
    {
        if(data.qs==QuestState.IN_PROGRESS)
        {
            QuestCheck.color = Color.gray;
        }else if (data.qs == QuestState.FINISHED)
        {
            QuestCheck.color = Color.red;
        }
        
    }

    public void DisplayQuestInfo()
    {
        QuestInfoPanel.setQuestInfo(questData);
    }
}
