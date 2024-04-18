using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms.Impl;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private TMP_Text top_scores;
    [SerializeField] private TMP_Text your_stats;
    private List<int> leaderboard;
    void OnEnable()
    {
        //StartCoroutine(ClearLeaderboard());
        //return;

        // Calculate score:
        int score;
        if (Manager.Instance.life > 60f)
        {
            // Game was won:
            score = Mathf.Max(500 - Manager.Instance.timer, 250);
        }
        else
        {
            // Game was lost:
            score = Manager.Instance.timer;
        }
        // Update your stats:
        if (!your_stats) { return; }
        your_stats.text += "Score: " + score + 
            "\nTime: " + Manager.Instance.timer + " sec"
            +"\nSticks: " + Manager.Instance.total_sticks
            + (Manager.Instance.isCheating ? "\n\n YOU CHEATED (no score submission)" : "");
        // Post the score and retrieve leaderboard:
        StartCoroutine(UpdateLeaderboard(score, Manager.Instance.isCheating));
    }

    IEnumerator UpdateLeaderboard(int score, bool isCheating)
    {
        // Construct the URI with the score
        string uri = "https://famous-gray-clownfish.cyclic.app/submit/" + (isCheating ? "" : score);

        // Create the UnityWebRequest
        using (UnityWebRequest uwr = UnityWebRequest.Post(uri, new WWWForm()))
        {
            // Send the request and wait for it to complete
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + uwr.error);
            }
            else
            {
                leaderboard = JsonUtility.FromJson<ScoreArray>(uwr.downloadHandler.text).scores.ToList<int>();
                // Generate leaderboard animation:
                if (!top_scores) { yield break; }
                top_scores.text = "";
                int rank = 1;
                foreach (int top_score in leaderboard)
                {
                    top_scores.text += rank + ": " + top_score + (top_score == score ? "  <-- you" : "") + "\n";
                    rank++;
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }

    IEnumerator ClearLeaderboard()
    {
        string uri = "https://famous-gray-clownfish.cyclic.app/delete/";
        using (UnityWebRequest uwr = UnityWebRequest.Delete(uri))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + uwr.error);
            }
            else
            {
                // Handle the response as needed
                Debug.Log("Delete request completed successfully.");
            }
        }
    }


    [Serializable]
    public class ScoreArray
    {
        public int[] scores;
    }
}
