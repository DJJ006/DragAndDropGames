using System.Xml.Linq;
using UnityEngine;


public class CloudLayerFixer : MonoBehaviour
{
    private RectTransform rectTransform;

    [Tooltip("If set > 0, a child Canvas will be added/used and forced to this sortingOrder so clouds render above other UI.")]
    [SerializeField]
    private int forcedSortingOrder = 100;

    private Canvas cloudCanvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Drošības pārbaude
        if (rectTransform == null)
        {
            Debug.LogWarning($"{name}: CloudLayerFixer nevar atrast RectTransform!");
            return;
        }

        // Uzreiz noliek priekšplānā
        rectTransform.SetAsLastSibling();

        EnsureCanvasSorting();
    }

    void OnEnable()
    {
        // Gadījumā, ja prefab tiek aktivizēts vēlāk — arī pārliek priekšplānā
        rectTransform?.SetAsLastSibling();
        EnsureCanvasSorting();
    }

    void Update()
    {
        // Ja kāds cits skripts izmaina kārtību, mākoņi ik sekundi atgriežas priekšplānā
        if (Time.frameCount % 60 == 0) // ik ~1s
        {
            rectTransform?.SetAsLastSibling();
            EnsureCanvasSorting();
        }
    }

    // Ensure there's a Canvas on this GameObject (nested canvas) that overrides sorting and uses a high sorting order.
    private void EnsureCanvasSorting()
    {
        if (forcedSortingOrder <= 0)
            return;

        // Use existing Canvas on this GameObject if present, otherwise add one.
        if (cloudCanvas == null)
            cloudCanvas = GetComponent<Canvas>();

        if (cloudCanvas == null)
            cloudCanvas = gameObject.AddComponent<Canvas>();

        // Force the nested canvas to render on top
        cloudCanvas.overrideSorting = true;
        cloudCanvas.sortingOrder = forcedSortingOrder;
    }
}