using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Collect History
/// </summary>
public class CollectRecord
{
    public PrimitiveType CollectType; // Model Shape.

    public float CollectValue; // Collection Influence.

    public int CollectLevel; // In Which Level was it collected? 
}

public class ScoresManager : SingleBehaviour<ScoresManager>
{
    #region Serializable Fields

    public Text LevelScore;

    #endregion

    #region Global Variables

    private float scoreValue; // individual level score.

    private ResultsManager resultManager;

    private LevelsManager levelManager;

    private SoundsManager beepManager;

    #endregion

    #region Unity Lifecycle

    IEnumerator Start()
    {
        yield return new WaitUntil(() =>
        {
            resultManager = ResultsManager.Instance;
            return resultManager;
        });

        yield return new WaitUntil(() =>
        {
            levelManager = LevelsManager.Instance;
            return levelManager;
        });

        yield return new WaitUntil(() =>
        {
            beepManager = SoundsManager.Instance;
            return beepManager;
        });
    }

    #endregion

    #region Public Callbacks

    public void ReceiveCollectable(PrimitiveType collectType, float collectMass)
    {
        var collectRecords = resultManager.CollectRecords;
        var changeValue = 0f;

        if (collectRecords.Count < 1)
        {
            changeValue = 20f * collectMass;
            beepManager.PlayBeep(EnumBeeps.credits);
        }   
        else
        {
            var lastRecord = collectRecords.LastOrDefault();
            if (lastRecord != null)
            {
                var lastType = lastRecord.CollectType;
                if (collectType == lastType) // collect same type models twice in a row.
                {
                    var lastValue = lastRecord.CollectValue;
                    changeValue = - Mathf.Abs(lastValue) - 20f * collectMass; // take 2 most recent model credits off.  
                    beepManager.PlayBeep(EnumBeeps.damage);
                }
                else
                {
                    changeValue = 20f * collectMass;
                    beepManager.PlayBeep(EnumBeeps.credits);
                }
            }
        }

        scoreValue += changeValue;
        var currentLevel = levelManager.ActiveIndex + 1;
        collectRecords.Add(
        new CollectRecord
        {
            CollectType = collectType,
            CollectValue = changeValue,
            CollectLevel = currentLevel
        });

        var levelValue = Mathf.RoundToInt(scoreValue);
        LevelScore.text = string.Format("Level:{0}", levelValue);
        resultManager.RefreshBoard(changeValue);

        var oneHundred = Mathf.RoundToInt(scoreValue) / 100;
        if (oneHundred > 0) // achieve 100 score proceed to next level.
        {
            var nextLevel = currentLevel + 1;
            if (nextLevel > 4)
                return;

            resultManager.LevelTitleUp(nextLevel);
            levelManager.LoadNewLevel(string.Format("Level {0} Scene", nextLevel));
        }
        //else if (scoreValue < 0)
        //{
        //    var prevLevel = levelManager.ActiveIndex;
        //    if (prevLevel < 1)
        //        return;

        //    levelManager.LoadNewLevel(string.Format("Level {0} Scene", prevLevel));
        //}
    }

    #endregion
}
