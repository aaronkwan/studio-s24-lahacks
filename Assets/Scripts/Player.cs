using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject stick;
    [SerializeField] private GameObject lamp_off;
    [SerializeField] private GameObject lamp_on;

    #region Movement

    [SerializeField] private Rigidbody2D body;
    public float baseSpeed = 0.1f;
    public float currentSpeed = 0.1f;
    private Controller controller => Manager.Instance.m_controller;

    private void FixedUpdate()
    {
        // Movement:
        Vector2 direction = controller.GetDirectionInput();
        body.AddForce(direction * currentSpeed);
        Manager.Instance.currentSpeed = (direction == Vector2.zero) ? 0 : 
            direction.magnitude * currentSpeed;

        // Rotation:
        if (direction != Vector2.zero)
        {
            float targetAngle = (direction == Vector2.up) ? 0 : (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
            float currentAngle = transform.rotation.eulerAngles.z;
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, 0.2f);
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }

        // Casting:
        if (controller.GetCastInput())
        {
            if (flashLightCoroutine == null && !hasStick)
            {
                flashLightCoroutine = StartCoroutine(FlashLightCoroutine());
                Manager.Instance.m_sound.FlashLight();
            }
        }
        // Sprinting:
        if (controller.GetSprintInput())
        {
            currentSpeed = Mathf.Min(currentSpeed + 0.001f, baseSpeed * 1.8f);
        }
        else
        {
            currentSpeed = Mathf.Max(baseSpeed, currentSpeed - 0.002f);
        }
    }
    #endregion

    #region Interaction

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "ExitCave")
        {
            Manager.Instance.ExitCave();
            return;
        }
        if (collision.gameObject.name == "EnterCave")
        {
            Manager.Instance.EnterCave();
            return;
        }
        if (collision.gameObject.name == "Stick(Clone)")
        {
            PickUpStick();
            Destroy(collision.gameObject);
            Manager.Instance.SpawnNewStick();
            return;
        }
        if (collision.gameObject.name == "Campfire" && hasStick)
        {
            DropStick();
            Manager.Instance.life += 10f;
            return;
        }

    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Grass")
        {
            Manager.Instance.currentGround = Manager.GROUND.GRASS;
            return;
        }
        if (collision.gameObject.name == "Gravel")
        {
            Manager.Instance.currentGround = Manager.GROUND.GRAVEL;
            return;
        }
        if (collision.gameObject.name == "Stone")
        {
            Manager.Instance.currentGround = Manager.GROUND.STONE;
            return;
        }
    }

    private Coroutine flashLightCoroutine;
    private bool hasStick = false;
    IEnumerator FlashLightCoroutine()
    {
        // Flash Lamp, idle for 4 seconds, then reset.
        lamp_on.SetActive(true);
        lamp_off.SetActive(false);
        Manager.Instance.showSticks = true;
        yield return new WaitForSeconds(0.5f);
        lamp_on.SetActive(false);
        lamp_off.SetActive(false);
        Manager.Instance.showSticks = false;
        yield return new WaitForSeconds(4f);
        lamp_off.SetActive(true);
        flashLightCoroutine = null;
    }
    private void PickUpStick()
    {
        Manager.Instance.m_sound.PickupStick();

        hasStick = true;
        if (flashLightCoroutine != null)
        {
            StopCoroutine(flashLightCoroutine);
            flashLightCoroutine = null;
            Manager.Instance.showSticks = false;
        }
        lamp_on.SetActive(false);
        lamp_off.SetActive(false);
        stick.SetActive(true);
    }
    private void DropStick()
    {
        Manager.Instance.m_sound.DropStick();

        hasStick = false;
        stick.SetActive(false);
        lamp_on.SetActive(false);
        lamp_off.SetActive(true);
    }
    #endregion
}
