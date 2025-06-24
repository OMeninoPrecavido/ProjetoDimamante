using System.Collections;
using UnityEngine;

public class CameraMovement10 : MonoBehaviour
{
    #region References

    //Serialized references
    [SerializeField] Transform player;  //Player character

    //Component references
    PlayerMovement playerMovement; //Player's movement script

    #endregion

    #region Properties

    //Serialized Properties

        //Camera zones and limits
    [SerializeField][Range(0, 1)] float playerHZonePercentageEach;
    [SerializeField][Range(0, 1)] float spaceBetweenHZonesPercentage;

    [SerializeField][Range(0, 1)] float playerVLimitPosition;

        //Camera shift duration - when changing player framing
    [SerializeField] float shiftDuration;

        //Camera smoothing duration
    [SerializeField] float smoothTimeX;
    [SerializeField] float smoothTimeY;

        //Camera bounds
    [SerializeField] float leftBound;
    [SerializeField] float rightBound;
    [SerializeField] float upperBound;
    [SerializeField] float lowerBound;

    //Current camera focus
    Side currentFocus = Side.Left; //Auxiliates with camera bounds and setting the X movement orientation

    //Horizontal camera zone viewport limits
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
    
    float halfCamWidthWorld;        //Half the camera's size in world
    float halfCamHeightWorld;       //Half the camera's height in world

    float areaShiftOrientation; //Orientation when shifting areas

    //Movement states
    bool isChangingAreas;

    //Auxiliaries
    Vector3 auxSDX = Vector3.zero; //Follows the player in smooth damping
    float velocityX = 0;
    float velocityY = 0;

    #endregion

    #region Event Functions

    private void Start()
    {
        BoxCollider2D playerBC2D = player.GetComponentInChildren<BoxCollider2D>();

        playerWidthWorld = playerBC2D.bounds.size.x;                        //Assigns player width in world
        halfPlayerWidthWorld = playerWidthWorld / 2;                        //Assigns half player width in world
        halfPlayerWidthVP = GetWidthWorldToVP(halfPlayerWidthWorld);        //Assigns half player width in VP

        halfCamWidthWorld = Camera.main.orthographicSize * Camera.main.aspect;  //Gets half the camera's width
        halfCamHeightWorld = Camera.main.orthographicSize;

        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        //Limits for player horizontal zones on camera in viewport coordinates
        leftOuterLimitX = 0.5f - (spaceBetweenHZonesPercentage / 2) - playerHZonePercentageEach;
        leftInnerLimitX = 0.5f - (spaceBetweenHZonesPercentage / 2);

        rightOuterLimitX = 0.5f + (spaceBetweenHZonesPercentage / 2) + playerHZonePercentageEach;
        rightInnerLimitX = 0.5f + (spaceBetweenHZonesPercentage / 2);

        //Gets half the inner zone's width
        halfInnerZoneWidthWorld = GetWidthVPToWorld(spaceBetweenHZonesPercentage) / 2;

        //Gets offset used for the cam's framing
        camMoveOffsetWorld = halfInnerZoneWidthWorld + halfPlayerWidthWorld;

        //Updates the auxiliary following Vector3, so that its position can be copied by the camera
        auxSDX.x = Mathf.SmoothDamp(auxSDX.x, player.position.x, ref velocityX, smoothTimeX);

        //Raycasting for debugging
        DrawLimitsInEditor();
    }

    //Camera movement is done in LateUpdate so it is applies after every other transform change has already happened
    private void LateUpdate()
    {
        //Player's position in viewport coordinates
        Vector3 playerPosVP = Camera.main.WorldToViewportPoint(player.position);

        HandleHorizontalMovement(playerPosVP);

        HandleVerticalMovement(playerPosVP);
    }

    #endregion

    #region Horizontal Movement

    //Horizontal camera movement logic
    private void HandleHorizontalMovement(Vector3 playerPosVP)
    {
        //Player's left and right bounds X coordinates
        float playerLeftXVP = playerPosVP.x - halfPlayerWidthVP;
        float playerRightXVP = playerPosVP.x + halfPlayerWidthVP;

        if (!isChangingAreas)
        {
            //Camera's movement orientation in X axis
            float camMoveOrientationX = currentFocus == Side.Left ? 1 : -1; //If cam is framing the player on
                                                                            //the left, it's movement orientation
                                                                            //will be right

            //Player entered inbetween both player zones
            if (playerRightXVP > leftInnerLimitX && playerLeftXVP < rightInnerLimitX)
            {
                //Ensures camera stays inside bounds
                Vector3 newPos = AuxSDInst(camMoveOffsetWorld, camMoveOrientationX);
                if (newPos.x - halfCamWidthWorld >= leftBound && newPos.x + halfCamWidthWorld <= rightBound)
                    transform.position = newPos; //Cam follows closely with SmoothDamping               
            }

            //Changes the current camera focus in case the player is forced onto another area,
            //such as when the camera hits a bound and can't follow
            if (playerLeftXVP >= rightInnerLimitX)
                currentFocus = Side.Right;

            if (playerRightXVP <= leftInnerLimitX)
                currentFocus = Side.Left;

            //Player wondered outside one of the outer limits
            if (playerLeftXVP < leftOuterLimitX || playerRightXVP > rightOuterLimitX)
            {
                isChangingAreas = true;
                areaShiftOrientation = -camMoveOrientationX;
                StartCoroutine(AuxSDLerp(shiftDuration, areaShiftOrientation, camMoveOffsetWorld));
            }
        }
    }

    //Used to lerp the camera to a new framing whenever the player tries to go outwards
    private IEnumerator AuxSDLerp(float duration, float orientation, float offset)
    {
        float startX = transform.position.x;
        float timeElapsed = 0;
        float targetX = 0;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;

            //Smoothstep function
            t = t * t * (3f - 2f * t);

            targetX = auxSDX.x + orientation * offset;

            //Ensures camera stays within bounds
            if (targetX <= leftBound + halfCamWidthWorld)
                targetX = leftBound + halfCamWidthWorld;
                
            if (targetX >= rightBound - halfCamWidthWorld)
                targetX = rightBound - halfCamWidthWorld;

            float lerpedX = Mathf.Lerp(startX, targetX, t);
            transform.position = new Vector3(lerpedX, transform.position.y, transform.position.z);
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        currentFocus = currentFocus == Side.Left ? Side.Right : Side.Left;
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

    #endregion

    #region Vertical Movement

    private void HandleVerticalMovement(Vector3 playerPosVP)
    {
        //Player is under the vertical limit
        if (playerPosVP.y < playerVLimitPosition)
            MoveCameraY();

        //Player is above the vertical limit and is already grounded
        if (playerPosVP.y > playerVLimitPosition && playerMovement.IsGrounded)
            MoveCameraY();
    }

    private void MoveCameraY()
    {
        float newY = Mathf.SmoothDamp(transform.position.y, player.position.y, ref velocityY, smoothTimeY);

        //Checks for cam's vertical boundaries
        if (newY + halfCamHeightWorld > upperBound)
            newY = upperBound - halfCamHeightWorld;

        if (newY - halfCamHeightWorld < lowerBound)
            newY = lowerBound + halfCamHeightWorld;

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    #endregion

    #region Methods

    //Draws the player zone limits in Unity editor
    private void DrawLimitsInEditor()
    {
        //Horizontal limits
        Vector3 rcLeftOuterLimitX = Camera.main.ViewportToWorldPoint(new Vector3(leftOuterLimitX, 1, 0));
        Vector3 rcLeftInnerLimitX = Camera.main.ViewportToWorldPoint(new Vector3(leftInnerLimitX, 1, 0));

        Vector3 rcRightOuterLimitX = Camera.main.ViewportToWorldPoint(new Vector3(rightOuterLimitX, 1, 0));
        Vector3 rcRightInnerLimitX = Camera.main.ViewportToWorldPoint(new Vector3(rightInnerLimitX, 1, 0));

        Debug.DrawRay(rcLeftOuterLimitX, Vector3.down * 10f, UnityEngine.Color.red, 0f, false);
        Debug.DrawRay(rcLeftInnerLimitX, Vector3.down * 10f, UnityEngine.Color.red, 0f, false);

        Debug.DrawRay(rcRightInnerLimitX, Vector3.down * 10f, UnityEngine.Color.blue, 0f, false);
        Debug.DrawRay(rcRightOuterLimitX, Vector3.down * 10f, UnityEngine.Color.blue, 0f, false);

        //Horizontal camera bounds
        Vector3 leftBoundLimit = new Vector3(leftBound, 8, 0);
        Vector3 rightBoundLimit = new Vector3(rightBound, 8, 0);

        Debug.DrawRay(leftBoundLimit, Vector3.down * 20f, UnityEngine.Color.green, 0f, false);
        Debug.DrawRay(rightBoundLimit, Vector3.down * 20f, UnityEngine.Color.green, 0f, false);

        //Vertical limit
        Vector3 rcYLimit = Camera.main.ViewportToWorldPoint(new Vector3(0, playerVLimitPosition, 0));
        Debug.DrawRay(rcYLimit, Vector3.right * 20f, Color.magenta, 0f, false);

        //Vertical camera bounds

        Vector3 upperBoundLimit = new Vector3(-8, upperBound, 0);
        Vector3 lowerBoundLimit = new Vector3(-8, lowerBound, 0);

        Debug.DrawRay(upperBoundLimit, Vector3.right * 20f, UnityEngine.Color.green, 0f, false);
        Debug.DrawRay(lowerBoundLimit, Vector3.right * 20f, UnityEngine.Color.green, 0f, false);
    }

    #endregion

    #region Enums

    public enum Side
    {
        Left,
        Right
    }

    #endregion
}

