using UnityEngine;

public class FPSCAMERA : MonoBehaviour
{
    public float minX = -60f;
    public float maxX = 60f;
    public float movementSpeed;

    public float speed;
    public float sensitivity;
    public Camera cam;

    float rotY = 0f;
    float rotX = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        rotX += Input.GetAxis("Mouse Y") * sensitivity;
        rotY += Input.GetAxis("Mouse X") * sensitivity;
        
        rotX = Mathf.Clamp(rotX, minX, maxX);
        
        Quaternion target = Quaternion.Euler(-rotX, rotY, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, speed * Time.deltaTime);
    }
}
