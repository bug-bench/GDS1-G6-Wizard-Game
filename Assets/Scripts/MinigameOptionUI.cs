using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class OptionUI : MonoBehaviour
{
    public string minigameName;
    public string sceneName;
    public Transform markerContainer;
    public TMP_Text votesText;
    public TMP_Text nameText;
    int voteCount;

    public void Setup(string name, string scene)
    {
        minigameName = name;
        sceneName = scene;

        if (nameText != null)
            nameText.text = name;

        SetVotes(0);
    }

    public void SetVotes(int count)
    {
        voteCount = count;
        UpdateVotes();
    }

    void UpdateVotes()
    {
        if (votesText != null)
            votesText.text = voteCount.ToString();
    }
}