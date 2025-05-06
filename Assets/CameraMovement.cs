using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    //References
    [SerializeField] Transform player;

    //Editor Parameters
    [SerializeField] float smoothTimeX; //Amount of smoothing in following aux

    [SerializeField][Range(0, 1)] float pZoneWidthVP; //Percentage of player zone in relation to viewport
    float pZoneWidthWorld;
    float pZoneHalfWidthWorld;
    float pZoneLeftBound;
    float pZoneRightBound;

    [SerializeField] float cameraOffset; //Offset camera has from center of screen
    float currCamOffset;

    [SerializeField] float cameraShiftDuration; //Time it takes to shift camera orientation

    //Aux SD - Follows the player in smoothdamping and is followed by camera
    Vector3 auxPos = Vector3.zero;
    float velocityX = 0;

    //Player Coords & Bounds
    float playerX;
    float playerLeftX;
    float playerRightX;
    float playerHalfWidth;

    //Camera Focus
    Side currFocus = Side.Left;

    private void Start()
    {
        HorizontalStart();
    }

    private void Update()
    {
        HorizontalUpdate();
        DrawLimitsInEditor();
    }

    private void LateUpdate()
    {
        HorizontalLateUpdate();
    }

    #region Horizontal Movement

    private void HorizontalStart()
    {

        auxPos = player.position;
        currCamOffset = cameraOffset;

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

    #endregion

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

    }

    private enum Side
    {
        Left,
        Right
    }
}
