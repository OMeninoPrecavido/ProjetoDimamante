using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] Transform player;

    [SerializeField] [Range(0, 1)] float playerZonePercentageEach;
    [SerializeField] [Range(0, 1)] float spaceBetweenZonesPercentage;

    private float leftOuterLimitX;
    private float leftInnerLimitX;
    private float rightOuterLimitX;
    private float rightInnerLimitX;

    [SerializeField] float camSpeed;
    [SerializeField] float fastCamSpeed;

    bool isChangingAreas;

    float halfPlayerWidthVP;

    Vector3 areaShiftOrientation;

    private void Start()
    {
        halfPlayerWidthVP = GetColliderWidthInViewport() / 2;
    }

    private void Update()
    {
        //Limits for player zones on camera
        leftOuterLimitX = 0.5f - (spaceBetweenZonesPercentage / 2) - playerZonePercentageEach;
        leftInnerLimitX = 0.5f - (spaceBetweenZonesPercentage / 2);

        rightOuterLimitX = 0.5f + (spaceBetweenZonesPercentage / 2) + playerZonePercentageEach;
        rightInnerLimitX = 0.5f + (spaceBetweenZonesPercentage / 2);

        //Player's position in viewport coordinates
        Vector3 playerPosVP = Camera.main.WorldToViewportPoint(player.position);
        float playerSpeed = Mathf.Abs(player.GetComponent<Rigidbody2D>().linearVelocityX);

        if (!isChangingAreas)
        {
            Vector3 camMoveOrientation = new Vector3(transform.position.x - player.position.x, 0, 0).normalized;

            if (playerPosVP.x + halfPlayerWidthVP> leftInnerLimitX && playerPosVP.x - halfPlayerWidthVP < rightInnerLimitX)
                transform.position = NewCameraPos(camMoveOrientation, playerSpeed + camSpeed);

            if (playerPosVP.x - halfPlayerWidthVP < leftOuterLimitX || playerPosVP.x + halfPlayerWidthVP > rightOuterLimitX)
            {
                isChangingAreas = true;
                areaShiftOrientation = -camMoveOrientation;
            }   
        }

        if (isChangingAreas)
        {
            if ((areaShiftOrientation.x > 0 && playerPosVP.x + halfPlayerWidthVP > leftInnerLimitX)
                || (areaShiftOrientation.x < 0 && playerPosVP.x - halfPlayerWidthVP < rightInnerLimitX))
                transform.position = NewCameraPos(areaShiftOrientation, playerSpeed + fastCamSpeed);

            else
                isChangingAreas = false;
        }

        //Raycasting for debugging

        Vector3 rcLeftOuterLimitX = Camera.main.ViewportToWorldPoint(new Vector3(leftOuterLimitX, 1, 0));
        Vector3 rcLeftInnerLimitX = Camera.main.ViewportToWorldPoint(new Vector3(leftInnerLimitX, 1, 0));

        Vector3 rcRightOuterLimitX = Camera.main.ViewportToWorldPoint(new Vector3(rightOuterLimitX, 1, 0));
        Vector3 rcRightInnerLimitX = Camera.main.ViewportToWorldPoint(new Vector3(rightInnerLimitX, 1, 0));
        
        Debug.DrawRay(rcLeftOuterLimitX, Vector3.down * 10f, UnityEngine.Color.red, 0f, false);
        Debug.DrawRay(rcLeftInnerLimitX, Vector3.down * 10f, UnityEngine.Color.red, 0f, false);

        Debug.DrawRay(rcRightInnerLimitX, Vector3.down * 10f, UnityEngine.Color.blue, 0f, false);
        Debug.DrawRay(rcRightOuterLimitX, Vector3.down * 10f, UnityEngine.Color.blue, 0f, false);
    }

    private Vector3 NewCameraPos(Vector3 normalOrientation, float speed)
    {
        Vector3 newPos = transform.position + normalOrientation * speed * Time.deltaTime;
        return newPos;
    }

    float GetColliderWidthInViewport()
    {
        BoxCollider2D bc2d = player.GetComponentInChildren<BoxCollider2D>();

        // Largura do BoxCollider2D em World Space
        float widthWorld = bc2d.bounds.size.x;

        // Posição do objeto no mundo
        Vector3 leftWorld = transform.position - new Vector3(widthWorld / 2, 0, 0);
        Vector3 rightWorld = transform.position + new Vector3(widthWorld / 2, 0, 0);

        // Converte para Viewport Space
        float leftViewport = Camera.main.WorldToViewportPoint(leftWorld).x;
        float rightViewport = Camera.main.WorldToViewportPoint(rightWorld).x;

        // Retorna a largura em Viewport Space
        return Mathf.Abs(rightViewport - leftViewport);
    }

}
