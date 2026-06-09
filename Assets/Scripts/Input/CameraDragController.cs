using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraDragController : MonoBehaviour
{
    [Header("Input Actions (InputMap)")]
    [SerializeField] private InputActionReference dragButtonAction;
    [SerializeField] private InputActionReference pointerPositionAction;
    [SerializeField] private InputActionReference scrollAction;

    [Header("Drag")]
    [SerializeField] private float dragSensitivity = 0.015f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 0.25f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 8f;

    [Header("Bounds Around Store Center (World XZ)")]
    [SerializeField] private Vector3 boundsCenter = Vector3.zero;
    [SerializeField] private float limitX = 12f;
    [SerializeField] private float limitZ = 12f;

    [Header("Elastic")]
    [SerializeField] private float elasticExtraDistance = 2.5f;
    [SerializeField] private float elasticResistance = 0.35f;
    [SerializeField] private float returnSpeed = 10f;

    private Camera cam;
    private bool isDragging;
    private Vector2 lastPointerPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (!cam.orthographic)
        {
            Debug.LogWarning("CameraDragController foi pensado para camera ortografica.");
        }
    }

    private void OnEnable()
    {
        EnableAction(dragButtonAction);
        EnableAction(pointerPositionAction);
        EnableAction(scrollAction);
    }

    private void OnDisable()
    {
        DisableAction(dragButtonAction);
        DisableAction(pointerPositionAction);
        DisableAction(scrollAction);
    }

    private void Update()
    {
        if (!ActionsAreValid())
            return;

        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
        {
            isDragging = false;
            return;
        }

        if (ShouldBlockCameraInput())
        {
            isDragging = false;
            return;
        }

        HandleZoom();
        HandleDrag();
        HandleElasticReturn();
    }

    private bool ActionsAreValid()
    {
        return dragButtonAction != null &&
               dragButtonAction.action != null &&
               pointerPositionAction != null &&
               pointerPositionAction.action != null &&
               scrollAction != null &&
               scrollAction.action != null;
    }

    private bool ShouldBlockCameraInput()
    {
        if (StaffRecruitmentManager.Instance != null && StaffRecruitmentManager.Instance.IsRecruitmentOpen)
            return true;

        return false;
    }

    private void HandleZoom()
    {
        Vector2 scrollValue = scrollAction.action.ReadValue<Vector2>();
        float scrollY = scrollValue.y;

        if (Mathf.Approximately(scrollY, 0f))
            return;

        cam.orthographicSize -= scrollY * zoomSpeed * Time.deltaTime;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    private void HandleDrag()
    {
        bool isPressed = dragButtonAction.action.IsPressed();
        Vector2 currentPointerPosition = pointerPositionAction.action.ReadValue<Vector2>();

        if (isPressed && !isDragging)
        {
            isDragging = true;
            lastPointerPosition = currentPointerPosition;
            return;
        }

        if (!isPressed && isDragging)
        {
            isDragging = false;
            return;
        }

        if (!isDragging)
            return;

        Vector2 screenDelta = currentPointerPosition - lastPointerPosition;
        lastPointerPosition = currentPointerPosition;

        Vector3 right = transform.right;
        Vector3 forward = transform.forward;

        right.y = 0f;
        forward.y = 0f;

        right.Normalize();
        forward.Normalize();

        Vector3 worldMove = (-right * screenDelta.x - forward * screenDelta.y) * dragSensitivity;

        Vector3 targetPosition = transform.position + worldMove;
        targetPosition = ApplyElasticBounds(targetPosition);

        transform.position = targetPosition;
    }

    private Vector3 ApplyElasticBounds(Vector3 targetPosition)
    {
        float minX = boundsCenter.x - limitX;
        float maxX = boundsCenter.x + limitX;
        float minZ = boundsCenter.z - limitZ;
        float maxZ = boundsCenter.z + limitZ;

        float extendedMinX = minX - elasticExtraDistance;
        float extendedMaxX = maxX + elasticExtraDistance;
        float extendedMinZ = minZ - elasticExtraDistance;
        float extendedMaxZ = maxZ + elasticExtraDistance;

        targetPosition.x = Mathf.Clamp(targetPosition.x, extendedMinX, extendedMaxX);
        targetPosition.z = Mathf.Clamp(targetPosition.z, extendedMinZ, extendedMaxZ);

        if (targetPosition.x < minX)
        {
            float overflow = minX - targetPosition.x;
            targetPosition.x = minX - overflow * elasticResistance;
        }
        else if (targetPosition.x > maxX)
        {
            float overflow = targetPosition.x - maxX;
            targetPosition.x = maxX + overflow * elasticResistance;
        }

        if (targetPosition.z < minZ)
        {
            float overflow = minZ - targetPosition.z;
            targetPosition.z = minZ - overflow * elasticResistance;
        }
        else if (targetPosition.z > maxZ)
        {
            float overflow = targetPosition.z - maxZ;
            targetPosition.z = maxZ + overflow * elasticResistance;
        }

        return targetPosition;
    }

    private void HandleElasticReturn()
    {
        if (isDragging)
            return;

        float minX = boundsCenter.x - limitX;
        float maxX = boundsCenter.x + limitX;
        float minZ = boundsCenter.z - limitZ;
        float maxZ = boundsCenter.z + limitZ;

        Vector3 current = transform.position;

        Vector3 clamped = new Vector3(
            Mathf.Clamp(current.x, minX, maxX),
            current.y,
            Mathf.Clamp(current.z, minZ, maxZ)
        );

        transform.position = Vector3.Lerp(transform.position, clamped, returnSpeed * Time.deltaTime);
    }

    private void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Enable();
    }

    private void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Disable();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 realSize = new Vector3(limitX * 2f, 0.1f, limitZ * 2f);
        Gizmos.DrawWireCube(
            new Vector3(boundsCenter.x, transform.position.y, boundsCenter.z),
            realSize
        );

        Gizmos.color = Color.yellow;

        Vector3 elasticSize = new Vector3(
            (limitX + elasticExtraDistance) * 2f,
            0.1f,
            (limitZ + elasticExtraDistance) * 2f
        );

        Gizmos.DrawWireCube(
            new Vector3(boundsCenter.x, transform.position.y, boundsCenter.z),
            elasticSize
        );
    }
}