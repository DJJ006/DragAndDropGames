using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HanoiGameManager : MonoBehaviour
{
    [Header("Scene References")]
    public Peg[] Pegs;                     // assign left->middle->right pegs
    public RectTransform DiskPrefab;       // UI disk prefab (Image + Disk script)
    public Transform DiskParent;           // parent (Canvas) for instantiated disks

    [Header("Disk Settings")]
    public int DiskCount = 3;              // number of disks generated
    public float DiskHeight = 40f;         // vertical spacing between disks
    public Sprite[] DiskSprites;           // 0 = largest, last = smallest

    [Header("Gameplay")]
    public bool autoShuffleStart = false;  // optional random start

    private List<Disk> disks = new List<Disk>();

    // actual spacing used based on disk sizes to avoid overlap
    private float actualDiskHeight;

    void Start()
    {
        InitializeGame();
    }


    // -----------------------------
    //      INITIALIZE GAME
    // -----------------------------
    public void InitializeGame()
    {
        ClearExisting();
        CreateDisks(DiskCount);

        if (autoShuffleStart)
        {
            StackRandomOnLeft();
        }
        else
        {
            StackDisksOnPeg(0);
        }

        HanoiUIManager.Instance?.ResetUI();
    }


    // -----------------------------
    //      CLEAR OLD OBJECTS
    // -----------------------------
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


    // -----------------------------
    //      CREATE DISKS (WITH IMAGES)
    // -----------------------------
    private void CreateDisks(int count)
    {
        if (DiskPrefab == null || DiskParent == null)
        {
            Debug.LogError("DiskPrefab or DiskParent is not assigned.");
            return;
        }

        if (DiskSprites == null || DiskSprites.Length < count)
        {
            Debug.LogError("DiskSprites array does not contain enough sprites for selected disk count.");
            return;
        }

        disks.Clear();

        float maxHeight = 0f;

        for (int i = 0; i < count; i++)
        {
            // Instantiate UI disk
            RectTransform rt = Instantiate(DiskPrefab, DiskParent);
            rt.name = $"Disk_{i + 1}";

            // Make sure Disk script exists
            Disk disk = rt.GetComponent<Disk>();
            if (disk == null)
                disk = rt.gameObject.AddComponent<Disk>();

            // Disk size (largest gets highest number)
            int size = count - i;
            disk.Initialize(size, this);

            // Assign correct sprite
            Image img = rt.GetComponent<Image>();
            if (img != null)
            {
                int spriteIndex = size - 1;    // 0 = largest
                img.sprite = DiskSprites[spriteIndex];
                img.SetNativeSize();
            }

            // Optional: adjust width dynamically
            float t = (float)size / (float)count;
            float minWidth = DiskPrefab.rect.width * 0.5f;
            float maxWidth = DiskPrefab.rect.width;
            float width = Mathf.Lerp(minWidth, maxWidth, t);

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            // Height from prefab or fallback
            float height = rt.rect.height > 0 ? rt.rect.height : DiskHeight;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            // track maximum height for spacing
            if (height > maxHeight) maxHeight = height;

            // Add disk to list
            disks.Add(disk);
        }

        // Ensure spacing at least as large as the tallest disk to avoid visual overlap
        actualDiskHeight = Mathf.Max(DiskHeight, maxHeight);
    }


    // -----------------------------
    //      STACK DISKS ON PEG
    // -----------------------------
    private void StackDisksOnPeg(int pegIndex)
    {
        if (pegIndex < 0 || pegIndex >= Pegs.Length) pegIndex = 0;

        disks.Sort((a, b) => b.Size.CompareTo(a.Size)); // largest first
        for (int i = 0; i < disks.Count; i++)
        {
            Pegs[pegIndex].PlaceAtBottom(disks[i], i, actualDiskHeight);
        }
    }

    // stack disks in a random order on the left peg (peg0)
    private void StackRandomOnLeft()
    {
        if (Pegs == null || Pegs.Length == 0) return;

        // create a shuffled copy of disks
        List<Disk> shuffled = new List<Disk>(disks);
        int n = shuffled.Count;
        System.Random rnd = new System.Random();
        for (int i = 0; i < n; i++)
        {
            int j = rnd.Next(i, n);
            var tmp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = tmp;
        }

        // place shuffled disks on left peg in order bottom->top using PlaceAtTop
        Peg left = Pegs[0];
        for (int i = 0; i < shuffled.Count; i++)
        {
            left.PlaceAtTop(shuffled[i], left.Count, actualDiskHeight);
        }
    }


    // -----------------------------
    //      MOVE DISK LOGIC
    // -----------------------------
    public bool TryMoveDisk(Peg fromPeg, Peg toPeg)
    {
        if (fromPeg == null || toPeg == null) return false;

        Disk moving = fromPeg.Peek();
        if (moving == null) return false;

        if (!toPeg.CanPlace(moving)) return false;

        fromPeg.Pop();
        int newIndex = toPeg.Count;
        toPeg.PlaceAtTop(moving, newIndex, actualDiskHeight);

        HanoiUIManager.Instance?.OnMoveMade();
        CheckWinCondition();
        return true;
    }


    // -----------------------------
    //      CHECK WIN CONDITION
    // -----------------------------
    private void CheckWinCondition()
    {
        Peg finalPeg = Pegs[Pegs.Length - 1];

        if (finalPeg.Count == disks.Count)
        {
            HanoiUIManager.Instance?.OnWin();
        }
    }


    // -----------------------------
    //      RESTART HELPER
    // -----------------------------
    public void Restart()
    {
        InitializeGame();
    }
}
