using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    string[] names = new string[3] { "HeavyFemale", "StandardFemale", "StrongFemale" };
    public GameObject HeavyFemale;
    public GameObject StandardFemale;
    public GameObject StrongFemale;
    public List<GameObject> charlist;
    
    // Start is called before the first frame update
    void Start()
    {
        charlist.Add(HeavyFemale);
        charlist.Add(StandardFemale);
        charlist.Add(StrongFemale);
        for (int i = 0; i < 3; ++i)
        {
            if (i == Counter.activeCC) charlist[i].SetActive(true);
            else charlist[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Counter.activeCC = (Counter.activeCC + 1) % 3;
            SceneManager.LoadScene(0);
        }
    }
}
