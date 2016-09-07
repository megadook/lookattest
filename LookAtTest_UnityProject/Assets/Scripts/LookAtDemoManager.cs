using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LookAtDemoManager : MonoBehaviour
{
    public CharacterLookAt[] characterLookAts;

    [Space(10)]
    public GameObject stationaryParent;
    public GameObject thirdPersonParent;

    [Space(10)]
    public GameObject stationaryTim;
    public GameObject stationaryEthan;

    [Space(10)]
    public Text switchDemoText; 


    private void Awake()
    {
        StartStationaryDemo();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchDemos();
        }
        else if(stationaryParent.activeSelf && Input.GetKeyDown(KeyCode.C))
        {
            SwitchCharacters();
        }
        else if(thirdPersonParent.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ShowCursor();
        }
        else if (thirdPersonParent.activeSelf && Input.GetMouseButtonDown(0))
        {
            HideCursor();
        }
    }

    private void SwitchDemos()
    {
        if (stationaryParent.activeSelf)
        {
            StartThirdPersonDemo();
        }
        else
        {
            StartStationaryDemo();
        }
    }

    private void SwitchCharacters()
    {
        if (stationaryTim.activeSelf)
        {
            stationaryTim.SetActive(false);
            stationaryEthan.SetActive(true);
        }
        else
        {
            stationaryEthan.SetActive(false);
            stationaryTim.SetActive(true);
        }
    }

    private void StartThirdPersonDemo()
    {
        stationaryParent.SetActive(false);
        thirdPersonParent.SetActive(true);

        switchDemoText.text = "third person walk demo";
        HideCursor();
    }

    private void StartStationaryDemo()
    {
        thirdPersonParent.SetActive(false);
        stationaryParent.SetActive(true);

        switchDemoText.text = "stationary demo";
        ShowCursor();
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
