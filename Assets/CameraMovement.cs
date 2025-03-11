using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    #region References

    //Serialized references
    [SerializeField] Transform player;  //Player character

    #endregion

    #region Properties

    //Serialized Properties

        //Camera zones
    [SerializeField][Range(0, 1)] float playerZonePercentageEach;
    [SerializeField][Range(0, 1)] float spaceBetweenZonesPercentage;

        //Camera shift duration - when changing player framing
    [SerializeField] float shiftDuration;

        //Camera smoothing duration
    [SerializeField] float smoothTime;

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
    Vector3 auxSDX = Vector3.zero;
    float velocity = 0;

    #endregion

    #region Event Functions

    private void Start()
    {
        playerWidthWorld = player.GetComponentInChildren<BoxCollider2D>().bounds.size.x; //Assigns player width in world
        halfPlayerWidthWorld = playerWidthWorld / 2;                                     //Assigns half player width in world
        halfPlayerWidthVP = GetWidthWorldToVP(halfPlayerWidthWorld);                     //Assigns half player width in VP
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

        //Updates the auxiliary following Vector3, so that its position can be copied by the camera
        auxSDX.x = Mathf.SmoothDamp(auxSDX.x, player.position.x, ref velocity, smoothTime);

        //Raycasting for debugging
        DrawLimitsInEditor();
    }

    //Camera movement is done in LateUpdate so it is applies after every other transform change has already happened
    private void LateUpdate()
    {
        //Player's position in viewport coordinates
        Vector3 playerPosVP = Camera.main.WorldToViewportPoint(player.position);

        //Player's left and right bounds X coordinates
        float playerLeftXVP = playerPosVP.x - halfPlayerWidthVP;
        float playerRightXVP = playerPosVP.x + halfPlayerWidthVP;

        if (!isChangingAreas)
        {
            //Camera's movement orientation in X axis
            float camMoveOrientationX = new Vector3(transform.position.x - auxSDX.x, 0, 0).normalized.x;
            if (camMoveOrientationX == 0)
                camMoveOrientationX = 1;

            //Player entered inbetween both player zones
            if (playerRightXVP > leftInnerLimitX && playerLeftXVP < rightInnerLimitX)
                transform.position = AuxSDInst(camMoveOffsetWorld, camMoveOrientationX);

            //Player wondered outside one of the outer limits
            if (playerLeftXVP < leftOuterLimitX || playerRightXVP > rightOuterLimitX)
            {
                isChangingAreas = true;
                areaShiftOrientation = -camMoveOrientationX;
                StartCoroutine(AuxSDLerp(shiftDuration, areaShiftOrientation, camMoveOffsetWorld));
            }
        }
    }

    #endregion

    #region Methods

    //Used to lerp the camera to a new framing whenever the player tries to go outwards
    private IEnumerator AuxSDLerp(float duration, float orientation, float offset)
    {
        float startX = transform.position.x;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;

            t = t * t * (3f - 2f * t);

            float targetX = auxSDX.x + orientation * offset;
            float lerpedX = Mathf.Lerp(startX, targetX, t);
            transform.position = new Vector3(lerpedX, transform.position.y, transform.position.z);
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        transform.position = new Vector3(auxSDX.x + orientation * offset, transform.position.y, transform.position.z);
        isChangingAreas = false;
    }

    //Sets the camera position to the position of an auxiliary object that follows the player using smooth damping
    //This makes it so the camera follows the player using smooth damping, but also allows for the lerping method
    //to go straigth into smooth damping movement.
    //Used when player goes inwards
    private Vector3 AuxSDInst(float offset, float orientation)
    {
        float newPosX = auxSDX.x + orientation * offset;
        return new Vector3(newPosX, transform.position.y, transform.position.z);
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

    #endregion
}

