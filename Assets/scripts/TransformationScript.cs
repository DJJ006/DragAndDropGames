using UnityEngine;

public class TransformationScript : MonoBehaviour
{
    public ObjectScript objScript;

    void Update()
    {
        if (objScript.lastDragged != null)
        {
            RectTransform rect = objScript.lastDragged.GetComponent<RectTransform>();

          
            if (Input.GetKey(KeyCode.Z))
                rect.Rotate(0, 0, Time.deltaTime * 45f);

            if (Input.GetKey(KeyCode.X))
                rect.Rotate(0, 0, -Time.deltaTime * 45f);

            Vector3 scale = rect.localScale;

            // Scale Y
            if (Input.GetKey(KeyCode.UpArrow) && scale.y < 2f)
                scale.y += 0.005f;

            if (Input.GetKey(KeyCode.DownArrow) && scale.y > 0.3f)
                scale.y -= 0.005f;

            // Scale X
            if (Input.GetKey(KeyCode.LeftArrow) && scale.x > 0.3f)
                scale.x -= 0.005f;

            if (Input.GetKey(KeyCode.RightArrow) && scale.x < 2f) 
                scale.x += 0.005f;

            rect.localScale = scale;
        }
    }
}
