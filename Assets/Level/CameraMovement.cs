using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    #region Properties

    //References
    [Header("-References-")]
    [SerializeField] Transform _player;
    
    PlayerMovement _playerMovement;

    //SmoothTime
    [Header("-Smooth Time-")]
    [SerializeField] float _regularSmoothTimeX; //Amount of smoothing in following aux
    [SerializeField] float _smoothTimeY; //Amount of smoothing in vertical camera movement

    public float _smoothTimeX { get; private set; } //Value holder
    public void SetSmoothTimeX(float smoothTimeX) => _smoothTimeX = smoothTimeX;

    //ShiftTime
    [Header("-Shift Time-")]
    [SerializeField] float _cameraShiftDuration; //Time it takes to shift camera orientation

    //Player zones
    [Header("-Player Zones-")]
    [SerializeField][Range(0, 1)] float _pZoneWidthVP; //Percentage of player zone in relation to viewport
    float _pZoneWidthWorld;
    float _pZoneHalfWidthWorld;
    float _pZoneLeftBound;
    float _pZoneRightBound;

    [SerializeField][Range(0, 1)] float _playerVLimit;
    float _playerVLimitWorld;

    [SerializeField] float _cameraOffset; //Offset camera has from center of screen
    float _currCamOffset;

    //Camera bounds
    [Header("-Camera Bounds-")]
    [SerializeField] float _leftCamLimit;
    [SerializeField] float _rightCamLimit;
    [SerializeField] float _topCamLimit;
    [SerializeField] float _bottomCamLimit;

    //Aux SD - Follows the player in smoothdamping and is followed by camera
    Vector3 _auxPos = Vector3.zero;
    float _velocityX = 0;

    //Player Coords & Bounds
    float _playerX;
    float _playerLeftX;
    float _playerRightX;
    float _playerHalfWidth;

    //Camera Coords & Bounds
    float _halfCamWidth;
    float _halfCamHeight;

    //Camera Focus
    public Side CurrFocus { get; private set; } = Side.Left;

    //Enablers
    bool _hMoveEnabled = true;
    bool _vMoveEnabled = true;
    public void EnableHMovement(bool b) => _hMoveEnabled = b;
    public void EnableVMovement(bool b) => _vMoveEnabled = b;

    float _velocityY; //Used for vertical SmoothDamping

    //Coroutine pointer
    public Coroutine _shiftingCoroutine;

    public bool IsDashShifting = false;

    //Aux vars for Y movement
    float _playerY;

    #endregion

    #region Event Functions

    private void Start()
    {
        _playerMovement = _player.GetComponent<PlayerMovement>();

        HorizontalStart();
        VerticalStart();
    }

    private void Update()
    {
        if (_hMoveEnabled)
            HorizontalUpdate();

        DrawLimitsInEditor();
    }

    private void LateUpdate()
    {
        if (_hMoveEnabled)
            HorizontalLateUpdate();

        if (_vMoveEnabled)
            VerticalLateUpdate();
    }

    #endregion

    #region Horizontal Movement

    private void HorizontalStart()
    {
        _halfCamWidth = Camera.main.orthographicSize * Camera.main.aspect;  //Gets half the camera's width

        _auxPos = _player.position;
        _currCamOffset = _cameraOffset;

        _smoothTimeX = _regularSmoothTimeX;

        BoxCollider2D playerBC2D = _player.GetComponentInChildren<BoxCollider2D>();
        _playerHalfWidth = playerBC2D.bounds.size.x / 2;
    }

    private void HorizontalUpdate()
    {
        //Sets values for player coordinates
        _playerX = _player.position.x;
        _playerLeftX = _playerX - _playerHalfWidth;
        _playerRightX = _playerX + _playerHalfWidth;

        //Sets values for player zone coordinates and measures
        _pZoneWidthWorld = GetWidthVPToWorld(_pZoneWidthVP);
        _pZoneHalfWidthWorld = Mathf.Round(_pZoneWidthWorld / 2 * 1000) / 1000;
        _pZoneLeftBound = _auxPos.x - _pZoneHalfWidthWorld; //Used only for drawing in editor
        _pZoneRightBound = _auxPos.x + _pZoneHalfWidthWorld; //

        float playerLeftDist = Mathf.Abs(_playerLeftX - _auxPos.x);
        float playerRightDist = Mathf.Abs(_playerRightX - _auxPos.x);

        //Player has left player zone
        if (playerLeftDist > _pZoneHalfWidthWorld || playerRightDist > _pZoneHalfWidthWorld)
        {
            int m = (_player.position.x > _auxPos.x) ? -1 : 1; //Offset multiplier

            float newAuxPosX = Mathf.SmoothDamp(_auxPos.x, _player.position.x + m * (_pZoneHalfWidthWorld - _playerHalfWidth),
                ref _velocityX, _smoothTimeX);

            _auxPos.x = newAuxPosX; //Changes aux position
        }
    }

    private void HorizontalLateUpdate()
    {
        float playerLeftDist = Mathf.Round(Mathf.Abs(_playerLeftX - _auxPos.x) * 1000) / 1000;
        float playerRightDist = Mathf.Round(Mathf.Abs(_playerRightX - _auxPos.x) * 1000) / 1000;

        //Player has left player zone
        if ((playerLeftDist > _pZoneHalfWidthWorld || playerRightDist > _pZoneHalfWidthWorld) && !IsDashShifting)
        {
            //Player changed direction to right
            if (_player.position.x < _auxPos.x && CurrFocus == Side.Left)
            {
                if (_shiftingCoroutine != null)
                    StopCoroutine(_shiftingCoroutine);

                _shiftingCoroutine = StartCoroutine(ShiftCam(Side.Right));
            }

            //Player changed direction to left
            if (_player.position.x > _auxPos.x && CurrFocus == Side.Right)
            {
                if (_shiftingCoroutine != null)
                    StopCoroutine(_shiftingCoroutine);

                _shiftingCoroutine = StartCoroutine(ShiftCam(Side.Left));
            }
        }

        Vector3 newCamPos = new Vector3(_auxPos.x + _currCamOffset, transform.position.y, transform.position.z);

        if (newCamPos.x > _leftCamLimit + _halfCamWidth && newCamPos.x < _rightCamLimit - _halfCamWidth)
            transform.position = newCamPos; //Updates camera position
    }

    //Shifts camera to different orientation gradually
    public IEnumerator ShiftCam(Side newFocus)
    {
        CurrFocus = newFocus;

        int m = (newFocus == Side.Left) ? 1 : -1;
        float newOffset = m * _cameraOffset;

        float timeElapsed = 0;
        float startingX = _currCamOffset;

        while (timeElapsed < _cameraShiftDuration)
        {
            float t = timeElapsed / _cameraShiftDuration;

            //Quadratic ease-out function 
            t = 1 - ((1 - t) * (1 - t));

            _currCamOffset = Mathf.Lerp(startingX, newOffset, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        _currCamOffset = newOffset;

        IsDashShifting = false;
    }

    public IEnumerator SmoothChangeSmoothX(float newValue, float changeTime)
    {
        float currSmoothX = _smoothTimeX;
        float elapsedTime = 0;
        while (elapsedTime < changeTime)
        {
            _smoothTimeX = Mathf.Lerp(currSmoothX, newValue, elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _smoothTimeX = newValue;
    }

    //To be called on other scripts to avoid error messages
    public void StartShiftCoroutine(Side side)
    {
        if (_shiftingCoroutine != null)
            StopCoroutine( _shiftingCoroutine );

        IsDashShifting = true;

        _shiftingCoroutine = StartCoroutine(ShiftCam(side));
    }

    #endregion

    #region Vertical Movement

    private void VerticalStart()
    {
        _halfCamHeight = Camera.main.orthographicSize;  //Gets half the camera's height

        BoxCollider2D playerBC2D = _player.GetComponentInChildren<BoxCollider2D>();
        //playerHalfHeight = playerBC2D.bounds.size.y / 2;
    }

    private void VerticalLateUpdate()
    {
        float currPlayerY = _player.position.y;

        if (_playerMovement.IsGrounded)
            _playerY = currPlayerY;

        _playerVLimitWorld = Camera.main.ViewportToWorldPoint(new Vector3(0, _playerVLimit, 0)).y;

        //Player is under the vertical limit
        if (currPlayerY < _playerVLimitWorld)
            MoveCameraY();

        //Player is above the vertical limit and is already grounded
        if (_playerY > _playerVLimitWorld)
            MoveCameraY();
    }

    private void MoveCameraY()
    {
        float newY = Mathf.SmoothDamp(transform.position.y, _player.position.y, ref _velocityY, _smoothTimeY);

        //Checks for cam's vertical boundaries
        if (newY + _halfCamHeight > _topCamLimit)
            newY = _topCamLimit - _halfCamHeight;

        if (newY - _halfCamHeight < _bottomCamLimit)
            newY = _bottomCamLimit + _halfCamHeight;

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
        Debug.DrawRay(_auxPos, Vector3.down * 3f, UnityEngine.Color.red, 0f, false);

        //PlayerZone bounds position
        Vector3 lbPos = new Vector3(_pZoneLeftBound, 0, 0);
        Vector3 rbPos = new Vector3(_pZoneRightBound, 0, 0);

        Debug.DrawRay(lbPos, Vector3.down * 3f, UnityEngine.Color.blue, 0f, false);
        Debug.DrawRay(rbPos, Vector3.down * 3f, UnityEngine.Color.blue, 0f, false);

        //Camera limits
        Debug.DrawRay(new Vector3(_leftCamLimit, 3, 0), Vector3.down * 5f, Color.green, 0f, false);
        Debug.DrawRay(new Vector3(_rightCamLimit, 3, 0), Vector3.down * 5f, Color.green, 0f, false);
        Debug.DrawRay(new Vector3(-3, _topCamLimit, 0), Vector3.right * 5f, Color.green, 0f, false);
        Debug.DrawRay(new Vector3(-3, _bottomCamLimit, 0), Vector3.right * 5f, Color.green, 0f, false);

        //Camera movement vertical limit
        Debug.DrawRay(new Vector3(transform.position.x-5, _playerVLimitWorld, 0), Vector3.right * 10f, Color.red, 0f, false);

    }

    #endregion
}
