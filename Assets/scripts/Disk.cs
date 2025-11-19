using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class Disk : MonoBehaviour
{
    public int Size { get; private set; }
    public Peg CurrentPeg { get; set; }
    HanoiGameManager manager;

    // Initialize called by manager when created
    public void Initialize(int size, HanoiGameManager gm)
    {
        Size = size;
        manager = gm;
        // ensure there's a TouchDrag component to allow dragging
        if (GetComponent<TouchDrag>() == null)
            gameObject.AddComponent<TouchDrag>();
    }
}