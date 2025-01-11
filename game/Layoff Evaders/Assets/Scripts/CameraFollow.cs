using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = target.position + offset;
        targetPos.x = 0;
        targetPos.y = transform.position.y;
        transform.position = targetPos;
    }
}
