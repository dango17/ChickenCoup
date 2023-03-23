using UnityEngine;

public class PointOfInterest : MonoBehaviour {
    public Vector3 StandPosition {
        get { return standPosition; }
        private set { standPosition = value; }
    }

    [SerializeField]
    private Vector3 standPosition = Vector3.zero;

    private void Start() {
        standPosition = transform.position - standPosition;
    }
}
