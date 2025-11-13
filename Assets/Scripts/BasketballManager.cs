using System.Data.Common;
using TMPro;
using UnityEngine;

public class BasketballManager : MonoBehaviour
{

    public TextMeshPro player1_name;
    public TextMeshPro player2_name;
    public TextMeshPro player1_score;
    public TextMeshPro player2_score;
    public string id1;
    public string id2;
    int score1;
    int score2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void inc_score(string id)
    {
        if (id == id1)
        {
            score1 += 1;
            player1_score.text = score1.ToString();
        }
        else if (id == id2)
        {
            score2 += 1;
            player2_score.text = score2.ToString();
        }

        if (id1 == "")
        {
            id1 = id;
            score1 += 1;
            player1_score.text = score1.ToString();
        }
        else if (id2 == "" && id != id1)
        {
            id2 = id;
            score2 += 1;
            player2_score.text = score2.ToString();
        }
    }
}
