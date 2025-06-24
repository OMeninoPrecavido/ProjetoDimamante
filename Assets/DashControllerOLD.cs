using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DashControllerOLD : MonoBehaviour
{
    [SerializeField] Transform starPrefab;

    PlayerMovement playerMovement;

    InputAction chargeDashAction;

    [SerializeField] float dashMaxDistance;
    [SerializeField] float dashStarSpeed;
    [SerializeField] float dashEffectDelay;

    Transform starRef;
    public bool dashIsCharging { get; private set; } = false;

    public delegate void Dash(Phase phase);
    public event Dash OnDash;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        chargeDashAction = InputSystem.actions.FindAction("ChargeDash");
        chargeDashAction.performed += StartChargeDash;
    }

    private void OnDisable()
    {
        chargeDashAction.performed -= StartChargeDash;
        //releaseDashAction.performed -= StopChargeDash;
    }

    private void StartChargeDash(InputAction.CallbackContext context)
    {
        starRef = Instantiate(starPrefab, transform.position, Quaternion.identity);
        dashIsCharging = true;
        StartCoroutine(MoveStar(playerMovement.PlayerOrientation));
    }

    private void StopChargeDash(InputAction.CallbackContext context) => StartCoroutine(StopChargeDashCoroutine());

    IEnumerator StopChargeDashCoroutine()
    {
        dashIsCharging = false;
        if (starRef != null)
        {
            Destroy(starRef.gameObject);

            OnDash.Invoke(Phase.Start);
            //Lock camera movement
            //Set camera x smooth time to something slower
            //Lock player movement
            //START DASH ANIM ENDS

            Vector3 newPlayerPos = starRef.position;
            transform.position = newPlayerPos;
            //FINAL DASH ANIM STARTS

            yield return new WaitForSeconds(1);

            OnDash.Invoke(Phase.End);
            //Unlock camera movement
            //Damage to enemies logic
            //Set camera smoothX back to normal assuming camera has already reached player

            //FINAL DASH ANIM ENDS  

            //Unlock player movement
        }
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
