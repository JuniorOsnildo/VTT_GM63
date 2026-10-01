using Core;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;

namespace VTT.Unity
{
    public class BoardCameraController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 160f;
        [SerializeField] private float zoomSpeed = 1f;
        [SerializeField] private float verticalSpeed = 22f;
        [SerializeField] private float focusSpeed = 45f;
        
        [SerializeField] private float minZoomDistance = 5f;
        [SerializeField] private float focusHeight = -1f;
        [SerializeField] private float minCameraY = 3f;
        
        [SerializeField] private GridManager gridManager;

        private Vector3 orbitCenter;
        private Vector3 initialPosition;
        
        private Vector3 boardCenter;
        
        private bool IsTypingInInputField()
        {
            if (EventSystem.current == null)
                return false;

            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

            if (selectedObject == null)
                return false;

            return selectedObject.GetComponent<TMP_InputField>() != null;
        }
        
        private void Start()
        {
            if (gridManager == null)
                return;

            gridManager.OnMapLoaded += HandleMapLoaded;

            if (gridManager.GetGrid() != null)
                HandleMapLoaded();
        }
        
        private void Update()
        {
            if (IsTypingInInputField())
                return;
            
            HandleMovement();
            HandleZoom();
            HandleReset();
            ClampCameraHeight();
        }
        
        private void HandleReset()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
                return;

            if (!keyboard.rKey.wasPressedThisFrame)
                return;

            orbitCenter = boardCenter;
            transform.position = initialPosition;
            transform.LookAt(orbitCenter);
        }
        
        private void HandleMapLoaded()
        {
            UpdateBoardCenter();

            BoardGrid grid = gridManager.GetGrid();

            if (grid == null)
                return;

            orbitCenter = boardCenter;

            float mapSize = Mathf.Max(grid.width, grid.height);

            float cameraHeight = mapSize;
            float cameraDistance = mapSize * 0.55f;

            transform.position = new Vector3(
                boardCenter.x,
                cameraHeight,
                boardCenter.z - cameraDistance
            );

            initialPosition = transform.position;

            transform.LookAt(orbitCenter);
        }
        
        private void UpdateBoardCenter()
        {
            BoardGrid grid = gridManager.GetGrid();

            if (grid == null)
                return;

            float centerX = (grid.width - 1) / 2f;
            float centerZ = (grid.height - 1) / 2f;

            boardCenter = new Vector3(centerX, focusHeight, centerZ);
        }
        

        private void HandleMovement()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
                return;

            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.aKey.isPressed)
                horizontal -= 1f;

            if (keyboard.dKey.isPressed)
                horizontal += 1f;

            if (keyboard.wKey.isPressed)
                vertical += 1f;

            if (keyboard.sKey.isPressed)
                vertical -= 1f;

            if (!Mathf.Approximately(horizontal, 0f))
            {
                transform.RotateAround(orbitCenter, Vector3.up, horizontal * moveSpeed * Time.deltaTime);
                transform.LookAt(orbitCenter);
            }

            if (!Mathf.Approximately(vertical, 0f))
            {
                Vector3 screenVertical = Vector3.ProjectOnPlane(transform.up, Vector3.up).normalized;

                Vector3 movement = screenVertical * vertical * verticalSpeed * Time.deltaTime;

                transform.position += movement;
                orbitCenter += movement;
            }
        }
        
        private void HandleZoom()
        {
            Mouse mouse = Mouse.current;

            if (mouse == null)
                return;

            float scroll = mouse.scroll.ReadValue().y;

            if (Mathf.Approximately(scroll, 0f))
                return;

            Camera camera = GetComponent<Camera>();

            if (camera == null)
                return;

            Ray ray = camera.ScreenPointToRay(mouse.position.ReadValue());
            Plane boardPlane = new Plane(Vector3.up, Vector3.zero);

            if (!boardPlane.Raycast(ray, out float enter))
                return;

            Vector3 mouseWorldPosition = ray.GetPoint(enter);
            Vector3 targetCenter = new Vector3(mouseWorldPosition.x, focusHeight, mouseWorldPosition.z);

            if (scroll > 0f)
            {
                Vector3 newOrbitCenter = Vector3.Lerp(orbitCenter, targetCenter, focusSpeed * Time.deltaTime);
                Vector3 centerMovement = newOrbitCenter - orbitCenter;

                orbitCenter = newOrbitCenter;

                float distance = Vector3.Distance(transform.position, orbitCenter);

                if (distance > minZoomDistance)
                {
                    transform.position += centerMovement;

                    Vector3 zoomDirection = transform.forward;
                    transform.position += zoomDirection * zoomSpeed;
                }
            }
            else
            {
                transform.position -= transform.forward * zoomSpeed;
            }
        }
        
        private void ClampCameraHeight()
        {
            if (transform.position.y >= minCameraY)
                return;

            float correction = minCameraY - transform.position.y;

            transform.position += Vector3.up * correction;
            orbitCenter += Vector3.up * correction;
        }
        
        private void OnDestroy()
        {
            if (gridManager != null)
                gridManager.OnMapLoaded -= HandleMapLoaded;
        }
    }
}