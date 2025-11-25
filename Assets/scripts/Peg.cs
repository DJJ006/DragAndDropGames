using System.Collections.Generic;
using UnityEngine;

public class Peg : MonoBehaviour
{
    private List<Disk> stack = new List<Disk>();

    public int Count => stack.Count;

    // Check if the peg accepts this disk (empty or top smaller than disk)
    public bool CanPlace(Disk disk)
    {
        if (disk == null) return false;
        Disk top = Peek();
        return top == null || disk.Size < top.Size;
    }

    public Disk Peek()
    {
        if (stack.Count == 0) return null;
        return stack[stack.Count - 1];
    }

    public void Pop()
    {
        if (stack.Count == 0) return;
        stack.RemoveAt(stack.Count - 1);
    }

    public void PlaceAtTop(Disk disk, int indexOnPeg, float diskHeight)
    {
        if (disk == null) return;
        stack.Add(disk);
        AttachDiskTransform(disk, stack.Count - 1, diskHeight);
        disk.CurrentPeg = this;
        disk.OnPlacedByPeg();
    }

    // Used for initial stacking where we place from bottom up
    public void PlaceAtBottom(Disk disk, int bottomIndex, float diskHeight)
    {
        if (disk == null) return;
        // bottomIndex: 0 -> bottom (largest), increasing upwards
        stack.Insert(0, disk);
        // After insertion, reposition all disks to match ordering
        RepositionAll(diskHeight);
        disk.CurrentPeg = this;
        disk.OnPlacedByPeg();
    }

    private void RepositionAll(float diskHeight)
    {
        for (int i = 0; i < stack.Count; i++)
        {
            AttachDiskTransform(stack[i], i, diskHeight);
        }
    }

    private void AttachDiskTransform(Disk disk, int indexOnPeg, float diskHeight)
    {
        RectTransform rt = disk.GetComponent<RectTransform>();
        rt.SetParent(transform, worldPositionStays: false);
        // place anchored at center x, y offset by index
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, indexOnPeg * diskHeight);
        disk.transform.SetAsLastSibling();
    }

    public void Clear()
    {
        stack.Clear();
    }
}