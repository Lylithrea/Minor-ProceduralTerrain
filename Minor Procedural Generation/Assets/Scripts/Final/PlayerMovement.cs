using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float jumpHeight = 500;
    public float jetpackStrength = 25;
    private Rigidbody body;
    float distToGround;
    float xRotate = 0.0f;
    float yRotate = 0.0f;
    public GameObject characterCamera;

    // Start is called before the first frame update
    void Start()
    {
        body = this.GetComponent<Rigidbody>();
        distToGround = GetComponent<Collider>().bounds.extents.y;
        Cursor.lockState = CursorLockMode.Locked;
    }


        // Update is called once per frame
    void Update()
    {

        body.AddForce(transform.right * speed * Input.GetAxis("Horizontal"));
        body.AddForce(transform.forward * speed * Input.GetAxis("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            body.AddForce(transform.up * jumpHeight);
        }

        if (Input.GetKey(KeyCode.E))
        {
            body.AddForce(transform.up * jetpackStrength);
        }

        //this.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X") * -1, 0) * Time.deltaTime * rotationSpeed, Space.World);

        xRotate += Input.GetAxis("Mouse X") * rotationSpeed * Time.fixedDeltaTime;
        yRotate += Input.GetAxis("Mouse Y") * rotationSpeed * Time.fixedDeltaTime;
        yRotate = Mathf.Clamp(yRotate, -90f, 90f);
        this.gameObject.transform.eulerAngles = new Vector3(0, xRotate, 0.0f);
        characterCamera.transform.eulerAngles = new Vector3(-yRotate, xRotate, 0.0f);


        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    bool IsGrounded()
    {
       return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.5f);
     }

}
