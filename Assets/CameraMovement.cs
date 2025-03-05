using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    #region Constants

    private const float SD_THRESHOLD = 0.001f;

    #endregion

    #region References

    //Serialized references
    [SerializeField] Transform player;

    #endregion

    #region Properties

    //Serialized Properties

        //Camera zones
    [SerializeField] [Range(0, 1)] float playerZonePercentageEach;
    [SerializeField] [Range(0, 1)] float spaceBetweenZonesPercentage;

        //Camera smoothing time
    [SerializeField] float smoothTime;
    [SerializeField] float shiftSmoothTime;

    //Value holders

        //Camera zone viewport limits
    private float leftOuterLimitX;
    private float leftInnerLimitX;
    private float rightOuterLimitX;
    private float rightInnerLimitX;

        //Widths
    float playerWidthWorld;         //Player's width in world
    float halfPlayerWidthVP;        //Half the player's width in viewport
    float halfPlayerWidthWorld;     //Half the player's width in world
    float halfInnerZoneWidthWorld;  //Width of zone between inner limits in world
    float camMoveOffsetWorld;       //Offset to be added to the camera SmoothDamp in world

    float areaShiftOrientation; //Orientation when shifting areeas

        //Movement states
    bool isChangingAreas;

    //Auxiliaries
    private Vector3 sdVelocity = Vector3.zero;

    #endregion

    private void Start()
    {
        //Gets half the player's width from his collider
        playerWidthWorld = player.GetComponentInChildren<BoxCollider2D>().bounds.size.x;
        halfPlayerWidthWorld = playerWidthWorld / 2;
        halfPlayerWidthVP = GetWidthWorldToVP(halfPlayerWidthWorld);
    }

    private void Update()
    {
        //Limits for player zones on camera in viewport coordinates
        leftOuterLimitX = 0.5f - (spaceBetweenZonesPercentage / 2) - playerZonePercentageEach;
        leftInnerLimitX = 0.5f - (spaceBetweenZonesPercentage / 2);

        rightOuterLimitX = 0.5f + (spaceBetweenZonesPercentage / 2) + playerZonePercentageEach;
        rightInnerLimitX = 0.5f + (spaceBetweenZonesPercentage / 2);

        //Gets half the inner zone's width
        halfInnerZoneWidthWorld = GetWidthVPToWorld(spaceBetweenZonesPercentage) / 2;

        //Gets offset used for the MoveCameraX method
        camMoveOffsetWorld = halfInnerZoneWidthWorld + halfPlayerWidthWorld;

        //Player's position in viewport coordinates
        Vector3 playerPosVP = Camera.main.WorldToViewportPoint(player.position);

        //Player's left and right bounds X coordinates
        float playerLeftXVP = playerPosVP.x - halfPlayerWidthVP;
        float playerRightXVP = playerPosVP.x + halfPlayerWidthVP;

        if (!isChangingAreas)
        {
            //Camera's movement orientation in X axis
            float camMoveOrientationX = new Vector3(transform.position.x - player.position.x, 0, 0).normalized.x;
            if (camMoveOrientationX == 0)
                camMoveOrientationX = 1;

            //Player entered inbetween both player zones
            if (playerRightXVP > leftInnerLimitX && playerLeftXVP < rightInnerLimitX)
                transform.position = MoveCameraX(camMoveOffsetWorld, camMoveOrientationX, smoothTime);

            //Player wondered outside one of the outer limits
            if (playerLeftXVP < leftOuterLimitX || playerRightXVP > rightOuterLimitX)
            {
                isChangingAreas = true;
                areaShiftOrientation = -camMoveOrientationX;
            }   
        }

        if (isChangingAreas)
        {
            //Player isn't yet on the correct camera zone, which depends on the orientation of the area shift
            if ((areaShiftOrientation > 0 && playerRightXVP > leftInnerLimitX)
                || (areaShiftOrientation < 0 && playerLeftXVP < rightInnerLimitX))
                transform.position = MoveCameraX(camMoveOffsetWorld, areaShiftOrientation, shiftSmoothTime);

            //Ensures the camera doesn't approach forever
            if (areaShiftOrientation > 0 && (playerRightXVP - leftInnerLimitX <= SD_THRESHOLD)
                || (areaShiftOrientation < 0 && rightInnerLimitX - playerLeftXVP <= SD_THRESHOLD))
                isChangingAreas = false;
        }

        //Raycasting for debugging
        DrawLimitsInEditor();        
    }

    //Moves the camera in the X axis
    private Vector3 MoveCameraX(float offset, float orientation, float sTime)
    {
        Vector3 targetPos = new Vector3(player.position.x + offset * orientation, transform.position.y, transform.position.z);
        return Vector3.SmoothDamp(transform.position, targetPos, ref sdVelocity, sTime);
    }

    //Converts a world width to viewport measurements
    private float GetWidthWorldToVP(float width)
    {
        //Extremity positions
        Vector3 leftWorld = transform.position - new Vector3(width / 2, 0, 0);
        Vector3 rightWorld = transform.position + new Vector3(width / 2, 0, 0);

        //Conversion to VP space
        float leftViewport = Camera.main.WorldToViewportPoint(leftWorld).x;
        float rightViewport = Camera.main.WorldToViewportPoint(rightWorld).x;

        return Mathf.Abs(rightViewport - leftViewport);
    }

    //Converts a viewport width to world measurements
    private float GetWidthVPToWorld(float widthVP)
    {
        Vector3 leftWorld = Camera.main.ViewportToWorldPoint(new Vector3(0.5f - widthVP / 2, 0, Camera.main.nearClipPlane));
        Vector3 rightWorld = Camera.main.ViewportToWorldPoint(new Vector3(0.5f + widthVP / 2, 0, Camera.main.nearClipPlane));

        return Mathf.Abs(rightWorld.x - leftWorld.x);
    }

    //Draws the player zone limits in Unity editor
    private void DrawLimitsInEditor()
    {
        Vector3 rcLeftOuterLimitX = Camera.main.ViewportToWorldPoint(new Vector3(leftOuterLimitX, 1, 0));
        Vector3 rcLeftInnerLimitX = Camera.main.ViewportToWorldPoint(new Vector3(leftInnerLimitX, 1, 0));

        Vector3 rcRightOuterLimitX = Camera.main.ViewportToWorldPoint(new Vector3(rightOuterLimitX, 1, 0));
        Vector3 rcRightInnerLimitX = Camera.main.ViewportToWorldPoint(new Vector3(rightInnerLimitX, 1, 0));

        Debug.DrawRay(rcLeftOuterLimitX, Vector3.down * 10f, UnityEngine.Color.red, 0f, false);
        Debug.DrawRay(rcLeftInnerLimitX, Vector3.down * 10f, UnityEngine.Color.red, 0f, false);

        Debug.DrawRay(rcRightInnerLimitX, Vector3.down * 10f, UnityEngine.Color.blue, 0f, false);
        Debug.DrawRay(rcRightOuterLimitX, Vector3.down * 10f, UnityEngine.Color.blue, 0f, false);
    }
}
