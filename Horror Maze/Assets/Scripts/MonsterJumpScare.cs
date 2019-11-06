using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterJumpScare : MonoBehaviour
{
    [SerializeField] private GameObject doll;
    [SerializeField] private GameObject ghost;
    [SerializeField] private GameObject brute; //henry

    public void Start()
    {
        HideAll();
    }

    public void Show(int type)
    {
        switch (type)
        {
            //ghost
            case 1:
                ghost.SetActive(true);
                break;

            //doll
            case 2:
                doll.SetActive(true);
                break;

            //brute (henry)
            case 3:
                brute.SetActive(true);
                break;

            default:
                break;
        }
    }

    public void Hide(int type)
    {
        switch (type)
        {
            //ghost
            case 1:
                ghost.SetActive(false);
                break;

            //doll
            case 2:
                doll.SetActive(false);
                break;

            //brute (henry)
            case 3:
                brute.SetActive(false);
                break;

            default:
                break;
        }
    }

    public void HideAll()
    {
        ghost.SetActive(false);
        doll.SetActive(false);
        brute.SetActive(false);
    }
}
