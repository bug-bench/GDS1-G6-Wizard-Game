using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class OptionUI : MonoBehaviour
{
    public string minigameName;
    public string sceneName;
    public Transform markerContainer;
    public TMP_Text descriptionText;
    public TMP_Text nameText;
    int voteCount;

    public void Setup(MinigameData data)
    {
        minigameName = data.minigameName;
        sceneName = data.sceneName;

        if (nameText != null)
            nameText.text = minigameName;

        if (descriptionText != null)
            descriptionText.text = data.description;

        SetVotes(0);
    }

    public void SetVotes(int count)
    {
        voteCount = count;
    }
}