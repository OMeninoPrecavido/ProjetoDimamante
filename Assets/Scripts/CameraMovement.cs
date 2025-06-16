using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    #region Properties

    //References
    [SerializeField] Transform player;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] DashController dashController;

    //Editor Parameters - SmoothTime
    [SerializeField] float regularSmoothTimeX; //Amount of smoothing in following aux
    [SerializeField] float smoothTimeY; //Amount of smoothing in vertical camera movement
    [SerializeField] float dashSmoothTimeX; //Amount of smoothing during dash
    float smoothTimeX;

    [SerializeField][Range(0, 1)] float pZoneWidthVP; //Percentage of player zone in relation to viewport
    float pZoneWidthWorld;
    float pZoneHalfWidthWorld;
    float pZoneLeftBound;
    float pZoneRightBound;

    [SerializeField][Range(0, 1)] float playerVLimit;
    float playerVLimitWorld;

    [SerializeField] float cameraOffset; //Offset camera has from center of screen
    float currCamOffset;

    [SerializeField] float cameraShiftDuration; //Time it takes to shift camera orientation

    //Camera bounds
    [SerializeField] float leftCamLimit;
    [SerializeField] float rightCamLimit;
    [SerializeField] float topCamLimit;
    [SerializeField] float bottomCamLimit;

    //Aux SD - Follows the player in smoothdamping and is followed by camera
    Vector3 auxPos = Vector3.zero;
    float velocityX = 0;

    //Player Coords & Bounds
    float playerX;
    float playerLeftX;
    float playerRightX;
    float playerHalfWidth;
    float playerHalfHeight;

    //Camera Coords & Bounds
    float halfCamWidth;
    float halfCamHeight;

    //Camera Focus
    Side currFocus = Side.Left;

    //Enablers
    bool hMoveEnabled = true;
    bool vMoveEnabled = true;

    float velocityY; //Used for vertical SmoothDamping

    #endregion

    #region Event Functions

    private void Start()
    {
        HorizontalStart();
        VerticalStart();
    }

    private void Update()
    {
        if (hMoveEnabled)
            HorizontalUpdate();

        DrawLimitsInEditor();
    }

    private void LateUpdate()
    {
        if (hMoveEnabled)
            HorizontalLateUpdate();

        if (vMoveEnabled)
            VerticalLateUpdate();
    }

    #endregion

    #region Horizontal Movement

    private void HorizontalStart()
    {
        dashController.OnDash += OnDashMade;

        halfCamWidth = Camera.main.orthographicSize * Camera.main.aspect;  //Gets half the camera's width

        auxPos = player.position;
        currCamOffset = cameraOffset;

        smoothTimeX = regularSmoothTimeX;

        BoxCollider2D playerBC2D = player.GetComponentInChildren<BoxCollider2D>();
        playerHalfWidth = playerBC2D.bounds.size.x / 2;
    }

    private void HorizontalUpdate()
    {
        //Sets values for player coordinates
        playerX = player.position.x;
        playerLeftX = playerX - playerHalfWidth;
        playerRightX = playerX + playerHalfWidth;

        //Sets values for player zone coordinates and measures
        pZoneWidthWorld = GetWidthVPToWorld(pZoneWidthVP);
        pZoneHalfWidthWorld = pZoneWidthWorld / 2;
        pZoneLeftBound = auxPos.x - pZoneHalfWidthWorld; //Used only for drawing in editor
        pZoneRightBound = auxPos.x + pZoneHalfWidthWorld; //

        float playerLeftDist = Mathf.Abs(playerLeftX - auxPos.x);
        float playerRightDist = Mathf.Abs(playerRightX - auxPos.x);

        //Player has left player zone
        if (playerLeftDist > pZoneHalfWidthWorld || playerRightDist > pZoneHalfWidthWorld)
        {
            int m = (player.position.x > auxPos.x) ? -1 : 1; //Offset multiplier

            float newAuxPosX = Mathf.SmoothDamp(auxPos.x, player.position.x + m * (pZoneHalfWidthWorld - playerHalfWidth),
                ref velocityX, smoothTimeX);

            auxPos.x = newAuxPosX; //Changes aux position
        }
    }

    private void HorizontalLateUpdate()
    {
        float playerLeftDist = Mathf.Abs(playerLeftX - auxPos.x);
        float playerRightDist = Mathf.Abs(playerRightX - auxPos.x);

        //Player has left player zone
        if (playerLeftDist > pZoneHalfWidthWorld || playerRightDist > pZoneHalfWidthWorld)
        {
            //Player changed direction to right
            if (player.position.x < auxPos.x && currFocus == Side.Left)
            {
                StartCoroutine(ShiftCam(Side.Right));
            }

            //Player changed direction to left
            if (player.position.x > auxPos.x && currFocus == Side.Right)
            {
                StartCoroutine(ShiftCam(Side.Left));
            }
        }

        Vector3 newCamPos = new Vector3(auxPos.x + currCamOffset, transform.position.y, transform.position.z);

        if (newCamPos.x > leftCamLimit + halfCamWidth && newCamPos.x < rightCamLimit - halfCamWidth)
            transform.position = newCamPos; //Updates camera position
    }

    //Shifts camera to different orientation gradually
    private IEnumerator ShiftCam(Side newFocus)
    {
        currFocus = newFocus;

        int m = (newFocus == Side.Left) ? 1 : -1;
        float newOffset = m * cameraOffset;

        float timeElapsed = 0;
        float startingX = currCamOffset;

        while (timeElapsed < cameraShiftDuration)
        {
            float t = timeElapsed / cameraShiftDuration;

            //Quadratic ease-out function 
            t = 1 - ((1 - t) * (1 - t));

            currCamOffset = Mathf.Lerp(startingX, newOffset, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void OnDashMade(Phase phase)
    {
        if (phase == Phase.Start)
        {
            hMoveEnabled = false;
            smoothTimeX = dashSmoothTimeX;
        }
        else
        if (phase == Phase.End)
        {
            hMoveEnabled = true;
        }
    }

    private IEnumerator ChangeSmoothXGradual(float value, float changeTime, float delay)
    {
        float elapsedTime = 0f;

        while (elapsedTime < delay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        float startingVal = smoothTimeX;

        while (elapsedTime < changeTime)
        {
            float val = elapsedTime / changeTime;
            smoothTimeX = Mathf.Lerp(smoothTimeX, value, val);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (smoothTimeX > value)
            smoothTimeX = value;
    }

    #endregion

    #region Vertical Movement

    private void VerticalStart()
    {
        halfCamHeight = Camera.main.orthographicSize;  //Gets half the camera's height

        BoxCollider2D playerBC2D = player.GetComponentInChildren<BoxCollider2D>();
        playerHalfHeight = playerBC2D.bounds.size.y / 2;
    }

    private void VerticalLateUpdate()
    {
        float playerY = player.position.y;
        playerVLimitWorld = Camera.main.ViewportToWorldPoint(new Vector3(0, playerVLimit, 0)).y;

        //Player is under the vertical limit
        if (playerY < playerVLimitWorld)
            MoveCameraY();

        //Player is above the vertical limit and is already grounded
        if (playerY > playerVLimitWorld && playerMovement.isGrounded)
            MoveCameraY();
    }

    private void MoveCameraY()
    {
        float newY = Mathf.SmoothDamp(transform.position.y, player.position.y, ref velocityY, smoothTimeY);

        //Checks for cam's vertical boundaries
        if (newY + halfCamHeight > topCamLimit)
            newY = topCamLimit - halfCamHeight;

        if (newY - halfCamHeight < bottomCamLimit)
            newY = bottomCamLimit + halfCamHeight;

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    #endregion

    #region Methods

    //Converts a viewport width to world measurements
    private float GetWidthVPToWorld(float widthVP)
    {
        Vector3 leftWorld = Camera.main.ViewportToWorldPoint(new Vector3(0.5f - widthVP / 2, 0, Camera.main.nearClipPlane));
        Vector3 rightWorld = Camera.main.ViewportToWorldPoint(new Vector3(0.5f + widthVP / 2, 0, Camera.main.nearClipPlane));

        return Mathf.Abs(rightWorld.x - leftWorld.x);
    }

    private void DrawLimitsInEditor()
    {
        //auxPos position
        Debug.DrawRay(auxPos, Vector3.down * 3f, UnityEngine.Color.red, 0f, false);

        //PlayerZone bounds position
        Vector3 lbPos = new Vector3(pZoneLeftBound, 0, 0);
        Vector3 rbPos = new Vector3(pZoneRightBound, 0, 0);

        Debug.DrawRay(lbPos, Vector3.down * 3f, UnityEngine.Color.blue, 0f, false);
        Debug.DrawRay(rbPos, Vector3.down * 3f, UnityEngine.Color.blue, 0f, false);

        //Camera limits
        Debug.DrawRay(new Vector3(leftCamLimit, 3, 0), Vector3.down * 5f, Color.green, 0f, false);
        Debug.DrawRay(new Vector3(rightCamLimit, 3, 0), Vector3.down * 5f, Color.green, 0f, false);
        Debug.DrawRay(new Vector3(-3, topCamLimit, 0), Vector3.right * 5f, Color.green, 0f, false);
        Debug.DrawRay(new Vector3(-3, bottomCamLimit, 0), Vector3.right * 5f, Color.green, 0f, false);

        //Camera movement vertical limit
        Debug.DrawRay(new Vector3(transform.position.x-5, playerVLimitWorld, 0), Vector3.right * 10f, Color.red, 0f, false);

    }

    #endregion
}
