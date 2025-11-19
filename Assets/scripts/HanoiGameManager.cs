using System.Collections.Generic;
using UnityEngine;

public class HanoiGameManager : MonoBehaviour
{
    [Header("Scene References")]
    public Peg[] Pegs;                     // assign left->middle->right pegs
    public RectTransform DiskPrefab;       // UI disk prefab (Image with Disk script)
    public int DiskCount = 3;              // default disks
    public float DiskHeight = 40f;         // vertical spacing between disks
    public Transform DiskParent;           // parent (Canvas) for instantiated disks

    [Header("Gameplay")]
    public bool autoShuffleStart = false;  // if true, randomize initial distribution (not standard Hanoi)

    private List<Disk> disks = new List<Disk>();

    void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        ClearExisting();
        CreateDisks(DiskCount);
        StackDisksOnPeg(0);
        HanoiUIManager.Instance?.ResetUI();
    }

    private void ClearExisting()
    {
        foreach (Peg p in Pegs)
            p.Clear();

        foreach (var d in disks)
        {
            if (d != null && d.gameObject != null)
                Destroy(d.gameObject);
        }
        disks.Clear();
    }

    private void CreateDisks(int count)
    {
        if (DiskPrefab == null || DiskParent == null) return;

        // Larger size means lower priority in stack (size 1 = smallest)
        for (int i = 0; i < count; i++)
        {
            RectTransform rt = Instantiate(DiskPrefab, DiskParent);
            rt.name = $"Disk_{i + 1}";
            Disk disk = rt.GetComponent<Disk>();
            if (disk == null) disk = rt.gameObject.AddComponent<Disk>();
            int size = count - i; // largest disk gets largest size number
            disk.Initialize(size, this);
            // scale width relative to size
            float t = (float)size / count;
            float minW = DiskPrefab.rect.width * 0.5f;
            float maxW = DiskPrefab.rect.width;
            float width = Mathf.Lerp(minW, maxW, t);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            disks.Add(disk);
        }
    }

    private void StackDisksOnPeg(int pegIndex)
    {
        if (Pegs == null || Pegs.Length == 0) return;
        if (pegIndex < 0 || pegIndex >= Pegs.Length) pegIndex = 0;

        // place largest at bottom (index 0 of disks is largest because created descending)
        disks.Sort((a, b) => b.Size.CompareTo(a.Size)); // largest first
        for (int i = 0; i < disks.Count; i++)
        {
            Pegs[pegIndex].PlaceAtBottom(disks[i], i, DiskHeight);
        }
    }

    public bool TryMoveDisk(Peg fromPeg, Peg toPeg)
    {
        if (fromPeg == null || toPeg == null) return false;
        Disk moving = fromPeg.Peek();
        if (moving == null) return false;

        if (!toPeg.CanPlace(moving)) return false;

        fromPeg.Pop();
        int newIndex = toPeg.Count;
        toPeg.PlaceAtTop(moving, newIndex, DiskHeight);

        HanoiUIManager.Instance?.OnMoveMade();
        CheckWinCondition();
        return true;
    }

    private void CheckWinCondition()
    {
        // standard win: all disks on last peg
        Peg finalPeg = Pegs[Pegs.Length - 1];
        if (finalPeg.Count == disks.Count)
        {
            HanoiUIManager.Instance?.OnWin();
        }
    }

    // Public helpers for UI
    public void Restart()
    {
        InitializeGame();
    }

    public Peg GetPegByTransform(Transform t)
    {
        foreach (var p in Pegs)
        {
            if (p != null && p.transform == t) return p;
        }
        return null;
    }
}