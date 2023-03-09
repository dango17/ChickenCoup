using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressuePadDoor : MonoBehaviour
{
    //Sliding door animation
    public AnimationCurve openSpeedCurve = new AnimationCurve(new Keyframe[]
    { new Keyframe(0, 1, 0, 0), new Keyframe(0.8f, 1, 0, 0), new Keyframe(1, 0, 0, 0) });

    public enum OpenDirection { x, y, z }
    public OpenDirection direction = OpenDirection.y;
    public float openDistance = 3f;
    public float openSpeedMultiplier = 2.0f;
    public Transform doorBody;

    bool open = false;

    Vector3 defaultDoorPosition;
    Vector3 currentDoorPosition;
    float openTime = 0;

    void Start()
    {
        if (doorBody)
        {
            defaultDoorPosition = doorBody.localPosition;
        }

        GetComponent<Collider>().isTrigger = true;
    }

    void Update()
    {
        if (!doorBody)
            return;

        if (openTime < 1)
        {
            openTime += Time.deltaTime * openSpeedMultiplier * openSpeedCurve.Evaluate(openTime);
        }

        if (direction == OpenDirection.x)
        {
            doorBody.localPosition = new Vector3(Mathf.Lerp(currentDoorPosition.x, defaultDoorPosition.x +
                (open ? openDistance : 0), openTime), doorBody.localPosition.y, doorBody.localPosition.z);
        }
        else if (direction == OpenDirection.y)
        {
            doorBody.localPosition = new Vector3(doorBody.localPosition.x, Mathf.Lerp(currentDoorPosition.y, defaultDoorPosition.y +
                (open ? openDistance : 0), openTime), doorBody.localPosition.z);
        }
        else if (direction == OpenDirection.z)
        {
            doorBody.localPosition = new Vector3(doorBody.localPosition.x, doorBody.localPosition.y, Mathf.Lerp(currentDoorPosition.z, defaultDoorPosition.z +
                (open ? openDistance : 0), openTime));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crates") || other.CompareTag("Player"))
        {
            open = true;
            currentDoorPosition = doorBody.localPosition;
            openTime = 0;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Crates") || other.CompareTag("Player"))
        {
            open = true;
            currentDoorPosition = doorBody.localPosition;
            openTime = 0;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Crates") || other.CompareTag("Player"))
        {
            open = false;
            currentDoorPosition = doorBody.localPosition;
            openTime = 0;
        }
    }
}