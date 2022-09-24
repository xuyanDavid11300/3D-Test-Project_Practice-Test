using UnityEngine;

// Unity Singleton Structure.
public class SingleBehaviour<SComponent> : MonoBehaviour where SComponent: SingleBehaviour<SComponent>
{
    public static SComponent Instance;

    protected virtual void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this as SComponent;
    }
}
