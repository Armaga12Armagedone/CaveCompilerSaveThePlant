using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraMove : MonoBehaviour
{
    [Header("Настройки движения")]
    public float keyboardSpeed = 15f;
    public float touchSpeed = 0.05f;
    public float smoothTime = 0.1f;
    
    [Header("Настройки зума")]
    public bool enableZoom = true;
    public float zoomSpeed = 5f;
    public float zoomSmoothTime = 0.1f;
    public float minZoom = 3f;
    public float maxZoom = 10f;
    public bool invertScroll = false;
    public float pinchZoomSensitivity = 0.01f;
    
    [Header("Границы (необязательно)")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Vector3 _targetPosition;
    private Vector3 _currentVelocity;
    private Vector2 _inputDelta;
    private Camera _camera;
    

    private float _targetOrthographicSize;
    private float _currentZoomVelocity;
    private float _initialTouchDistance;
    private float _initialZoomSize;
    private bool _isPinching = false;
    

    private Vector2 _lastTouchPosition;
    private bool _wasTouching = false;

    void Start()
    {
        _targetPosition = transform.position;
        _camera = Camera.main;
        
        if (_camera != null)
        {
            _targetOrthographicSize = _camera.orthographicSize;
        }
    }

    void Update()
    {

        bool hasTouches = Touchscreen.current != null && Touchscreen.current.touches.Count > 0;


        if (hasTouches)
        {
            HandleTouchInput();
        }
        else
        {

            HandleKeyboardMouseInput();
        }
        if (_inputDelta.sqrMagnitude > 0.001f && !_isPinching)
        {
            _targetPosition += (Vector3)_inputDelta;
        }
    }

    void LateUpdate()
    {

        Vector3 currentPos = transform.position;
        Vector3 targetWithZ = new Vector3(_targetPosition.x, _targetPosition.y, currentPos.z);
        transform.position = Vector3.SmoothDamp(currentPos, targetWithZ, ref _currentVelocity, smoothTime);
        

        if (_camera != null && enableZoom)
        {
            _camera.orthographicSize = Mathf.SmoothDamp(
                _camera.orthographicSize, 
                _targetOrthographicSize, 
                ref _currentZoomVelocity, 
                zoomSmoothTime
            );
        }
    }

    private void HandleKeyboardMouseInput()
    {
        _inputDelta = Vector2.zero;


        if (Keyboard.current != null)
        {
            Vector2 moveDir = Vector2.zero;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveDir.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveDir.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveDir.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveDir.x += 1;

            if (moveDir != Vector2.zero)
            {
                moveDir.Normalize();
                _inputDelta = moveDir * keyboardSpeed * Time.deltaTime;
            }
        }


        if (enableZoom && Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                float delta = invertScroll ? -scroll : scroll;
                _targetOrthographicSize -= delta * zoomSpeed * 0.1f;
                _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, minZoom, maxZoom);
            }
        }
    }

    private void HandleTouchInput()
    {
        var touches = Touchscreen.current.touches;
        int activeTouches = 0;
        foreach (var touch in touches)
            if (touch.press.isPressed) activeTouches++;


        if (activeTouches == 1 && !_isPinching)
        {

            var touch = GetPrimaryTouch();
            if (touch != null)
            {
                Vector2 currentPos = touch.position.ReadValue();

                if (!_wasTouching)
                {

                    _lastTouchPosition = currentPos;
                    _wasTouching = true;
                }
                else
                {

                    Vector2 delta = currentPos - _lastTouchPosition;
                    if (delta.magnitude < 100f)
                    {
                        _inputDelta = -delta * touchSpeed;
                    }
                    _lastTouchPosition = currentPos;
                }
            }
        }
        else if (activeTouches >= 2)
        {

            _inputDelta = Vector2.zero;
            _wasTouching = false;


            TouchControl touch1 = null;
            TouchControl touch2 = null;
            foreach (var touch in touches)
            {
                if (touch.press.isPressed)
                {
                    if (touch1 == null) touch1 = touch;
                    else if (touch2 == null) touch2 = touch;
                }
            }

            if (touch1 != null && touch2 != null)
            {
                Vector2 pos1 = touch1.position.ReadValue();
                Vector2 pos2 = touch2.position.ReadValue();
                float currentDistance = Vector2.Distance(pos1, pos2);

                if (!_isPinching)
                {

                    _initialTouchDistance = currentDistance;
                    _initialZoomSize = _targetOrthographicSize;
                    _isPinching = true;
                }
                else
                {

                    float distanceDelta = currentDistance - _initialTouchDistance;
                    float zoomDelta = distanceDelta * pinchZoomSensitivity;
                    if (invertScroll) zoomDelta = -zoomDelta;
                    _targetOrthographicSize = _initialZoomSize - zoomDelta;
                    _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, minZoom, maxZoom);
                }
            }
        }
        else
        {

            _inputDelta = Vector2.zero;
            _wasTouching = false;
            _isPinching = false;
        }
    }

    private TouchControl GetPrimaryTouch()
    {
        if (Touchscreen.current == null) return null;
        foreach (var touch in Touchscreen.current.touches)
            if (touch.press.isPressed) return touch;
        return null;
    }


    public void SetZoom(float zoomLevel)
    {
        if (enableZoom)
        {
            _targetOrthographicSize = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
        }
    }

    public float GetCurrentZoom() => _camera != null ? _camera.orthographicSize : 0f;
    public float GetTargetZoom() => _targetOrthographicSize;
    public void SetTargetPosition(Vector3 position) => _targetPosition = position;
    public Vector3 GetTargetPosition() => _targetPosition;
}