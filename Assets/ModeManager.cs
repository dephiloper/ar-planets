using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeManager : MonoBehaviour
{
    [SerializeField] private Image primaryActionImage;
    [SerializeField] private Button primaryActionButton;
    [SerializeField] private Button secondaryActionButton;
    [SerializeField] private Button modeButton;
    [SerializeField] private Image modeImage;
    [SerializeField] private Image pointerImage;

    public static ModeManager Instance { get; private set; }

    public enum Mode
    {
        Place = 0,
        Edit = 1,
        Simulate = 2,
    }

    public Sprite[] modeSprites;
    public Sprite[] primarySprites;

    public Mode CurrentMode { get; private set; } = Mode.Simulate;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        modeButton.onClick.AddListener(RotateMode);
        RotateMode();
    }

    private void RotateMode()
    {
        CurrentMode = (int)CurrentMode + 1 > 2 ? 0 : CurrentMode + 1;
        primaryActionImage.sprite = primarySprites[(int)CurrentMode];
        modeImage.sprite = modeSprites[(int)CurrentMode];
        secondaryActionButton.gameObject.SetActive(CurrentMode == Mode.Edit);
        pointerImage.gameObject.SetActive(CurrentMode == Mode.Edit);
    }

    public void ChangeGrabImage(bool isGrabbing)
    {
        primaryActionImage.sprite = primarySprites[isGrabbing ? 3 : 1];
    }

    public void ShowButtons(bool show)
    {
        primaryActionButton.gameObject.SetActive(show);
        secondaryActionButton.gameObject.SetActive(show);
        modeButton.gameObject.SetActive(show);
    }
}