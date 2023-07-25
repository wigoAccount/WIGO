using Crystal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveAreaService: MonoBehaviour
{
    [SerializeField] Image _topPanelMono;
    static Image _topPanel;

    SafeArea.SimDevice[] Sims;

    private void Awake()
    {
        _topPanel = _topPanelMono;
        Sims = (SafeArea.SimDevice[])Enum.GetValues(typeof(SafeArea.SimDevice));
    }

    public static void SetupTopPanel(Image background)
    {
        if (background == null)
        {
            _topPanel.sprite = null;
            _topPanel.color = Color.black;
        }
        else
        {
            _topPanel.sprite = background.sprite;
            _topPanel.color = background.color;
        }
    }

#if UNITY_EDITOR
    [SerializeField] KeyCode KeySafeArea = KeyCode.A;
    int SimIdx;

    void Update()
    {
        if (Input.GetKeyDown(KeySafeArea))
            ToggleSafeArea();
    }

    /// <summary>
    /// Toggle the safe area simulation device.
    /// </summary>
    void ToggleSafeArea()
    {
        SimIdx++;

        if (SimIdx >= Sims.Length)
            SimIdx = 0;

        SafeArea.Sim = Sims[SimIdx];
        Debug.LogFormat("Switched to sim device {0} with debug key '{1}'", Sims[SimIdx], KeySafeArea);
    }
#endif
}
