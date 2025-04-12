using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RectangleGenerator : MonoBehaviour
{
public Material rectangleMaterial;

    public float width = 2f;
    public float height = 1f;
    public float depth = 2f;
    public float focalLength = 10f;

    public float moveSpeed = 2f;
    public float mouseSensitivity = 0.2f;
    private Vector3 centerPosition = Vector3.zero;
    private Vector3 rectangleRotation;

    // Input fields
    public TMP_InputField inputWidth;
    public TMP_InputField inputHeight;
    public TMP_InputField inputDepth;
    public TMP_InputField inputScale;

    private float scale = 1f;
    private Vector2 previousMousePosition;

    void Update()
    {
        UpdateFromInputFields();
        HandleMouseRotation();
        HandleMovement();
    }

    private void OnPostRender()
    {
        if (rectangleMaterial == null) return;

        rectangleMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);

        Quaternion rotation = Quaternion.Euler(rectangleRotation);

        Vector3 scaledSize = new Vector3(width, height, depth) * scale;

        Vector3[] frontFace = GetRectangleFace(0f, rotation, centerPosition, scaledSize);
        Vector3[] backFace = GetRectangleFace(scaledSize.z, rotation, centerPosition, scaledSize);

        DrawQuad(frontFace);
        DrawQuad(backFace);

        for (int i = 0; i < 4; i++)
        {
            DrawLine(frontFace[i], backFace[i]);
        }

        GL.End();
        GL.PopMatrix();
    }

    private void UpdateFromInputFields()
    {
        if (inputWidth != null) float.TryParse(inputWidth.text, out width);
        if (inputHeight != null) float.TryParse(inputHeight.text, out height);
        if (inputDepth != null) float.TryParse(inputDepth.text, out depth);
        if (inputScale != null) float.TryParse(inputScale.text, out scale);
    }

    private void HandleMouseRotation()
    {
        if (Input.GetMouseButtonDown(0))
            previousMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - previousMousePosition;
            rectangleRotation.y += delta.x * mouseSensitivity;
            rectangleRotation.x -= delta.y * mouseSensitivity;
            previousMousePosition = Input.mousePosition;
        }
    }

    private void HandleMovement()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) input.y += 1;
        if (Input.GetKey(KeyCode.S)) input.y -= 1;
        if (Input.GetKey(KeyCode.A)) input.x -= 1;
        if (Input.GetKey(KeyCode.D)) input.x += 1;

        centerPosition += input * moveSpeed * Time.deltaTime;
    }

    private Vector3[] GetRectangleFace(float zOffset, Quaternion rotation, Vector3 center, Vector3 size)
    {
        float hw = size.x;
        float hh = size.y;

        Vector3[] corners = new Vector3[]
        {
            new Vector3(0, 0, zOffset),
            new Vector3(hw, 0, zOffset),
            new Vector3(hw, hh, zOffset),
            new Vector3(0, hh, zOffset)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = rotation * corners[i] + center;
            corners[i] = ApplyPerspective(corners[i]);
        }

        return corners;
    }

    private Vector3 ApplyPerspective(Vector3 worldPoint)
    {
        float scale = focalLength / (focalLength + worldPoint.z);
        return new Vector3(worldPoint.x * scale, worldPoint.y * scale, 0);
    }

    private void DrawQuad(Vector3[] corners)
    {
        for (int i = 0; i < 4; i++)
        {
            DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }

    private void DrawLine(Vector3 a, Vector3 b)
    {
        GL.Vertex3(a.x, a.y, a.z);
        GL.Vertex3(b.x, b.y, b.z);
    }
}
