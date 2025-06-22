using UnityEngine;

public class Pickup : MonoBehaviour
{
    public virtual void OnPicked()
    {
        Destroy(gameObject);
    }
}
