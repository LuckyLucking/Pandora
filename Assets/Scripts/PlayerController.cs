using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float dragPlaneZ = 0f;

    [Header("Pan")]
    [SerializeField] private bool enablePan = true;
    [SerializeField] private float panSensitivity = 1f;

    [Header("Zoom")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float mouseWheelZoomSpeed = 5f;
    [SerializeField] private float pinchZoomSpeed = 0.01f;
    [SerializeField] private float minOrthographicSize = 3f;
    [SerializeField] private float maxOrthographicSize = 30f;
    [SerializeField] private float minFieldOfView = 20f;
    [SerializeField] private float maxFieldOfView = 80f;

    [Header("Bounds")]
    [SerializeField] private bool clampPosition = false;
    [SerializeField] private Vector2 minCameraPosition = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 maxCameraPosition = new Vector2(150f, 150f);

    [Header("Input")]
    [SerializeField] private bool blockWhenPointerOverUI = true;

    private bool isMouseDragging;
    private Vector2 lastPointerScreenPosition;

    private void Reset()
    {
        CacheCamera();
    }

    private void Awake()
    {
        CacheCamera();
        ClampCameraState();
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            return;
        }

        if (UseTouchInput())
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }

        ClampCameraState();
    }

    private void CacheCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private bool UseTouchInput()
    {
        return Input.touchSupported && Input.touchCount > 0;
    }

    private void HandleMouseInput()
    {
        if (enablePan)
        {
            HandleMousePan();
        }

        if (enableZoom)
        {
            HandleMouseZoom();
        }
    }

    private void HandleMousePan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
            {
                isMouseDragging = false;
                return;
            }

            isMouseDragging = true;
            lastPointerScreenPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isMouseDragging = false;
        }

        if (!isMouseDragging || !Input.GetMouseButton(0))
        {
            return;
        }

        Vector2 currentPointerPosition = Input.mousePosition;
        PanFromScreenDelta(lastPointerScreenPosition, currentPointerPosition);
        lastPointerScreenPosition = currentPointerPosition;
    }

    private void HandleMouseZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        if (Mathf.Approximately(scrollDelta, 0f) || IsPointerOverUI())
        {
            return;
        }

        ZoomAroundScreenPoint(Input.mousePosition, -scrollDelta * mouseWheelZoomSpeed);
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount >= 2 && enableZoom)
        {
            HandlePinchZoom();
            return;
        }

        if (!enablePan)
        {
            return;
        }

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            if (IsPointerOverUI(touch.fingerId))
            {
                return;
            }

            lastPointerScreenPosition = touch.position;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            if (IsPointerOverUI(touch.fingerId))
            {
                return;
            }

            PanFromScreenDelta(lastPointerScreenPosition, touch.position);
            lastPointerScreenPosition = touch.position;
        }
    }

    private void HandlePinchZoom()
    {
        Touch firstTouch = Input.GetTouch(0);
        Touch secondTouch = Input.GetTouch(1);

        if (IsPointerOverUI(firstTouch.fingerId) || IsPointerOverUI(secondTouch.fingerId))
        {
            return;
        }

        Vector2 firstPrevious = firstTouch.position - firstTouch.deltaPosition;
        Vector2 secondPrevious = secondTouch.position - secondTouch.deltaPosition;

        float previousDistance = Vector2.Distance(firstPrevious, secondPrevious);
        float currentDistance = Vector2.Distance(firstTouch.position, secondTouch.position);
        float distanceDelta = currentDistance - previousDistance;

        Vector2 pinchCenter = (firstTouch.position + secondTouch.position) * 0.5f;
        ZoomAroundScreenPoint(pinchCenter, -distanceDelta * pinchZoomSpeed);
        lastPointerScreenPosition = pinchCenter;
    }

    private void PanFromScreenDelta(Vector2 previousScreenPosition, Vector2 currentScreenPosition)
    {
        Vector3 previousWorld = ScreenToWorldOnDragPlane(previousScreenPosition);
        Vector3 currentWorld = ScreenToWorldOnDragPlane(currentScreenPosition);
        Vector3 worldDelta = previousWorld - currentWorld;

        targetCamera.transform.position += worldDelta * panSensitivity;
    }

    private void ZoomAroundScreenPoint(Vector2 screenPoint, float zoomDelta)
    {
        Vector3 beforeZoomWorldPoint = ScreenToWorldOnDragPlane(screenPoint);

        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize = Mathf.Clamp(
                targetCamera.orthographicSize + zoomDelta,
                minOrthographicSize,
                maxOrthographicSize);
        }
        else
        {
            targetCamera.fieldOfView = Mathf.Clamp(
                targetCamera.fieldOfView + zoomDelta,
                minFieldOfView,
                maxFieldOfView);
        }

        Vector3 afterZoomWorldPoint = ScreenToWorldOnDragPlane(screenPoint);
        Vector3 correction = beforeZoomWorldPoint - afterZoomWorldPoint;
        targetCamera.transform.position += correction;
    }

    private Vector3 ScreenToWorldOnDragPlane(Vector2 screenPosition)
    {
        Ray ray = targetCamera.ScreenPointToRay(screenPosition);

        if (targetCamera.orthographic)
        {
            Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(targetCamera.transform.position.z - dragPlaneZ));
            return targetCamera.ScreenToWorldPoint(screenPoint);
        }

        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, dragPlaneZ));
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return targetCamera.transform.position;
    }

    private void ClampCameraState()
    {
        if (!clampPosition)
        {
            return;
        }

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.x = Mathf.Clamp(cameraPosition.x, minCameraPosition.x, maxCameraPosition.x);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, minCameraPosition.y, maxCameraPosition.y);
        targetCamera.transform.position = cameraPosition;
    }

    private bool IsPointerOverUI()
    {
        return blockWhenPointerOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsPointerOverUI(int fingerId)
    {
        return blockWhenPointerOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
    }
}
