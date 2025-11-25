using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class Disk : MonoBehaviour
{
    public int Size { get; private set; }
    public Peg CurrentPeg { get; set; }
    HanoiGameManager manager;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    // Initialize called by manager when created
    public void Initialize(int size, HanoiGameManager gm)
    {
        Size = size;
        manager = gm;
        // ensure there's a TouchDrag component to allow dragging
        if (GetComponent<TouchDrag>() == null)
            gameObject.AddComponent<TouchDrag>();

        // ensure Rigidbody2D exists
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        // configure rigidbody for UI behaviour: default to kinematic so it doesn't fall
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // ensure BoxCollider2D exists and roughly matches RectTransform size
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider2D>();

        // size the collider to the RectTransform in local space
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null && boxCollider != null)
        {
            // BoxCollider2D.size is in local space units. RectTransform.rect gives size in local space.
            boxCollider.size = rt.rect.size;
            boxCollider.offset = Vector2.zero;
        }
    }

    // Called by Peg when this disk has been attached to a peg
    public void OnPlacedByPeg()
    {
        // start a short physics settling so disks don't visually overlap
        if (rb != null)
            StartCoroutine(PlacedRoutine());
    }

    private IEnumerator PlacedRoutine()
    {
        // enable physics simulation briefly so collisions can resolve
        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // wait a short time for collisions to resolve
        yield return new WaitForSeconds(0.12f);

        // freeze and stop simulation to allow UI positioning logic to take over
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;
    }
}