using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemInfo
{
    public string CollectLevel;

    public int ContributeScore;
}

[Serializable]
public class ItemResults
{
    public string ItemType;

    public List<ItemInfo> ItemInfos;
}

[Serializable]
public class ResultReport
{
    public float TimeOfAttempt;

    public int AmountOfPushedObjects;

    public List<ItemResults> Score;
}

public class ResultsManager : GlobalBehaviour<ResultsManager>
{
    #region Serializable Fields

    public Text ResultText;

    #endregion

    #region Global Variables

    private float totalScore;

    private float startTime;

    private LevelsManager levelManager;

    #endregion

    #region Properties

    public List<CollectRecord> CollectRecords { get; set; } = new List<CollectRecord>();

    #endregion

    #region Unity Lifecycle

    IEnumerator Start()
    {
        startTime = Time.time;
        yield return new WaitUntil(() => levelManager = LevelsManager.Instance);
    }

    #endregion

    #region Functions / Methods

    public void RefreshBoard(float changeValue)
    {
        totalScore += changeValue;
        var wholeScore = Mathf.RoundToInt(totalScore);
        ResultText.text = string.Format("Total:{0}", wholeScore);

        if (wholeScore >= 400)
            FinalizeReport();
    }

    public void FinalizeReport()
    {
        var elapseTime = Time.time - startTime;
        var collectCount = CollectRecords.Count;

        // classify by item type, and then by level(from begin to end), and total the scores.
        var itemsScore = CollectRecords.GroupBy(record =>
            record.CollectType).Select(type =>
                new ItemResults
                {
                    ItemType = Enum.GetName(typeof(PrimitiveType), type.Key),
                    ItemInfos = type.GroupBy(g => g.CollectLevel)
                     .Select(level =>
                     new ItemInfo
                     {
                         CollectLevel = string.Format("Level {0}", level.Key),
                         ContributeScore = Mathf.RoundToInt(level.Sum(score => score.CollectValue))
                     })
                     .OrderBy(item => item.CollectLevel)
                     .ToList()
                }).ToList();

        var resultReport =
        new ResultReport
        {
            TimeOfAttempt = elapseTime,
            Score = itemsScore,
            AmountOfPushedObjects = collectCount
        };

        var resultJson = JsonUtility.ToJson(resultReport);
        var resultPath = Path.Combine(Application.persistentDataPath, "Datas", "ResultReport.json");

        File.WriteAllText(resultPath, resultJson);
        RestartGame();
    }

    private void RestartGame()
    {
        startTime = Time.time;
        totalScore = 0f;
        ResultText.text = "Total:0";

        if (CollectRecords != null)
            CollectRecords.Clear();

        else
            CollectRecords = new List<CollectRecord>();

        levelManager.LoadNewLevel("Level 1 Scene");
    }

    #endregion
}
