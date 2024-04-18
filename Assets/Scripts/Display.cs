using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Display : MonoBehaviour
{
    [SerializeField] public GameObject pauseMenu;
    [SerializeField] public GameObject gameOverMenu;
    [SerializeField] public GameObject gameWonMenu;
    [SerializeField] private Image overlayLife;

    public void DisplayPause(bool pause)
    {
        pauseMenu.SetActive(pause);
    }
    public void DisplayGameOver(bool won)
    {
        if (won)
        {
            gameWonMenu.SetActive(won);
        }
        else
        {
            gameOverMenu.SetActive(!won);
        }
    }
    void FixedUpdate()
    {
        overlayLife.fillAmount = (Manager.Instance.life / 60f);
    }

    #region Mobile

    [SerializeField] private GameObject startCanvas;
    [SerializeField] private GameObject touchCanvas;
    [SerializeField] private RectTransform touchCanvasRect;
    [SerializeField] private RectTransform touchMarker_start;
    [SerializeField] private RectTransform touchMarker_move;
    public bool isHoldingSprint = false;
    public bool isHoldingCast = false;
    public bool isHoldingPause = false;
    private bool isHoldingMove = false;

    public void StartGame(bool isMobile)
    {
        startCanvas.SetActive(false);
        if (isMobile)
        {
            touchCanvas.SetActive(true);
        }
    }
    public void SetTouchMarker(Vector2 pos, bool show)
    {
        if (show)
        {
            if (isHoldingMove)
            {
                // Continue to move:
                // Calculate the offset from the start marker to the current position:
                Vector2 localPos = touchMarker_start.transform.position;
                Vector2 offset = (new Vector2(pos.x - localPos.x, pos.y - localPos.y));
                if (offset.magnitude > 45f)
                {
                    offset.Normalize();
                    offset *= 45f;
                }
                Vector2 screenPoint = localPos + offset;
                // Convert this to UI space:
                RectTransformUtility.ScreenPointToLocalPointInRectangle(touchCanvasRect, screenPoint, null, out localPos);
                touchMarker_move.transform.localPosition = localPos;
            }
            else
            {
                // Just started moving:
                // Convert screen point to UI space:
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(touchCanvasRect, new Vector2(pos.x, pos.y), null, out localPos);
                touchMarker_start.transform.localPosition = localPos;
                touchMarker_move.transform.localPosition = localPos;
            }
            isHoldingMove = true;
        }
        else
        {
            // Hide the markers:
            touchMarker_start.anchoredPosition = new Vector2(-2000, -2000);
            touchMarker_move.anchoredPosition = new Vector2(-2000, -2000);
            isHoldingMove = false;
        }
    }
    
    // Called by buttons event triggers:
    public void HoldSprint(bool hold)
    {
        isHoldingSprint = hold;
    }
    public void HoldCast(bool hold)
    {
        isHoldingCast = hold;
    }
    public void HoldPause(bool value)
    {
        isHoldingPause = value;
    }




    #endregion Mobile
}
