using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Controller : MonoBehaviour
{
    #region Controls
    private abstract class Control
    {
        public abstract Vector2 GetDirectionInput();
        public abstract bool GetCastInput();
        public abstract bool GetSprintInput();
        public abstract bool GetPauseInput();
        public abstract bool GetCheatPlusInput();
        public abstract bool GetCheatMinusInput();
        public abstract void Update();
    }
    private class KeyboardControl : Control
    {
        private bool isPausing = false;
        public override Vector2 GetDirectionInput()
        {
            Vector2 direction = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
            {
                direction += Vector2.up;
            }
            if (Input.GetKey(KeyCode.S))
            {
                direction += Vector2.down;
            }
            if (Input.GetKey(KeyCode.A))
            {
                direction += Vector2.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                direction += Vector2.right;
            }
            return direction.normalized;
        }
        public override bool GetCastInput()
        {
            return Input.GetKey(KeyCode.Space);
        }
        public override bool GetSprintInput()
        {
            return Input.GetKey(KeyCode.LeftShift);
        }
        public override bool GetPauseInput()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                if (!isPausing)
                {
                    isPausing = true;
                    return true;
                }
                return false;
            }
            isPausing = false;
            return false;
        }
        public override bool GetCheatPlusInput()
        {
            return Input.GetKey(KeyCode.KeypadPlus);
        }
        public override bool GetCheatMinusInput()
        {
            return Input.GetKey(KeyCode.KeypadMinus);
        }
        public override void Update()
        {
            // No updates needed for keyboard controls.
        }
    }
    private class TouchControl : Control
    {
        public TouchControl(Vector2 initial_touch) { start = initial_touch; }

        private Vector2 start;
        private Vector2 move;
        private Vector2 direction;
        private int firstTouch;
        private Display Display => Manager.Instance.m_display;

        public override Vector2 GetDirectionInput()
        {
            return direction.normalized;
        }
        public override bool GetCastInput()
        {
            return Display.isHoldingCast;
        }
        public override bool GetSprintInput()
        {
            return Display.isHoldingSprint;
        }
        public override bool GetPauseInput()
        {
            bool pause = Display.isHoldingPause;
            if (pause)
            {
                Display.HoldPause(false);
                return true;
            }
            return false;
        }
        public override bool GetCheatPlusInput()
        {
            return false;
        }
        public override bool GetCheatMinusInput()
        {
            return false;
        }
        private void ProcessTouch(int fingerId)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId != firstTouch)
                {
                    continue;
                }
                move = t.position;
                switch (t.phase)
                {
                    case TouchPhase.Began:
                        start = t.position;
                        break;
                    case TouchPhase.Ended:
                        direction = Vector2.zero;
                        break;
                    case TouchPhase.Canceled:
                    case TouchPhase.Moved:
                        Vector2 moved = t.position - start;
                        if (moved.magnitude < 15f)
                        {
                            direction = Vector2.zero;
                        }
                        else
                        {
                            direction = moved;
                        }
                        break;
                    case TouchPhase.Stationary:
                        break;
                }
            }
        }
        public override void Update()
        {
            // Reset if pause or untouched:
            if (Input.touchCount == 0 || Manager.Instance.isPaused)
            {
                firstTouch = -1;
                direction = Vector2.zero;
                Manager.Instance.m_display.SetTouchMarker(Vector2.zero, false);
                return;
            }
            // Get the first touch:
            if (Input.touchCount == 1)
            {
                firstTouch = Input.GetTouch(0).fingerId;
            }
            ProcessTouch(firstTouch);
            Manager.Instance.m_display.SetTouchMarker(move, true);
        }
    }
    private class NullControl : Control
    {
        public override bool GetCastInput()
        {
            return false;
        }
        public override bool GetCheatMinusInput()
        {
            return false;
        }

        public override bool GetCheatPlusInput()
        {
            return false;
        }

        public override Vector2 GetDirectionInput()
        {
            return Vector2.zero;
        }

        public override bool GetPauseInput()
        {
            return false;
        }

        public override bool GetSprintInput()
        {
            return false;
        }

        public override void Update(){}
    }

    #endregion Controls

    #region Interface

    private Control control;
    private bool isCasting;

    private void Start()
    {
        control = new NullControl();
    }
    public Vector2 GetDirectionInput() { return control.GetDirectionInput(); }
    public bool GetSprintInput() { return control.GetSprintInput(); }
    public bool GetCastInput()
    {
        // Return true if the player has queued a cast that has not been processed yet:
        if (control.GetCastInput())
        {
            if (!isCasting)
            {
                isCasting = true;
                return true;
            }
            return false;
        }
        isCasting = false;
        return false;
    }
    private void FixedUpdate()
    {
        // Pausing:
        if (control.GetPauseInput())
        {
            Manager.Instance.TogglePause();
        }
        // Cheats:
        if (control.GetCheatPlusInput())
        {
            Manager.Instance.isCheating = true;
            Manager.Instance.life += 0.5f;
        }
        if (control.GetCheatMinusInput())
        {
            Manager.Instance.isCheating = true;
            Manager.Instance.life -= 0.5f;
        }
    }
    private void Update()
    {
        if (control is NullControl)
        {
            // Start Game as mobile or desktop:
            if (Input.touchCount > 0)
            {
                Manager.Instance.StartGame(true);
                control = new TouchControl(Input.GetTouch(0).position);
                return;
            }
            if (Input.anyKeyDown)
            {
                Manager.Instance.StartGame(false);
                control = new KeyboardControl();
            }
            return;
        }
        control.Update();
    }

    #endregion Interface
}
