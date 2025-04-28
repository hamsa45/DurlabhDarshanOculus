using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldSpaceUIInteractor : MonoBehaviour
{
    [Header("References")]
    public Camera eventCamera; // Camera to cast ray from (usually your Main Camera)
    public LayerMask uiLayerMask; // LayerMask for your UI (set your UI to a specific Layer)

    [Header("Settings")]
    public float maxDistance = 10f; // Max distance for interaction

    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private GraphicRaycaster raycaster;

    private void Awake()
    {
        eventSystem = EventSystem.current;
        raycaster = FindObjectOfType<GraphicRaycaster>(); // Get the first GraphicRaycaster
    }

    private void Update()
    {
        if (eventSystem == null || raycaster == null)
            return;

        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        // Create a list of Raycast Results
        var results = new System.Collections.Generic.List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            // We are hovering over UI
            if (Input.GetMouseButtonDown(0)) // Left Click
            {
                foreach (var result in results)
                {
                    var button = result.gameObject.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.Invoke(); // Manually trigger click
                        Debug.Log("Clicked on " + button.gameObject.name);
                    }
                }
            }
        }
        else
        {
            // Optional: You can also Raycast into 3D space if needed
            Ray ray = eventCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, uiLayerMask))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Clicked on 3D Object: " + hit.collider.name);
                    // If your world UI has colliders, you can detect them here
                }
            }
        }
    }
}
