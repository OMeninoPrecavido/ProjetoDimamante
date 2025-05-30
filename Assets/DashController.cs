using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DashController : MonoBehaviour
{
    [SerializeField] Transform starPrefab;

    PlayerMovement playerMovement;

    InputAction chargeDashAction;
    InputAction releaseDashAction;

    [SerializeField] float dashMaxDistance;
    [SerializeField] float dashStarSpeed;
    Transform starRef;
    bool dashIsCharging = false;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        chargeDashAction = InputSystem.actions.FindAction("ChargeDash");
        chargeDashAction.performed += StartChargeDash;

        releaseDashAction = InputSystem.actions.FindAction("ReleaseDash");
        releaseDashAction.performed += StopChargeDash;
    }

    private void OnDisable()
    {
        chargeDashAction.performed -= StartChargeDash;
        releaseDashAction.performed -= StopChargeDash;
    }

    private void StartChargeDash(InputAction.CallbackContext context)
    {
        starRef = Instantiate(starPrefab, transform.position, Quaternion.identity);
        dashIsCharging = true;
        StartCoroutine(MoveStar(playerMovement.PlayerOrientation));
    }

    private void StopChargeDash(InputAction.CallbackContext context)
    {
        Destroy(starRef.gameObject);
        dashIsCharging = false;
    }

    IEnumerator MoveStar(int orientation)
    {
        float playerStartDist = Mathf.Abs(starRef.position.x - transform.position.x);
        while (playerStartDist < dashMaxDistance && dashIsCharging)
        {
            Vector3 newPos = new Vector3(starRef.position.x + (dashStarSpeed * Time.deltaTime * orientation), 
                                         starRef.position.y, starRef.position.z);

            starRef.position = newPos;
            playerStartDist = Mathf.Abs(starRef.position.x - transform.position.x);

            yield return null;
        }
    }
}
