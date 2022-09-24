// inherit singleton features plus persistency across scenes.
public class GlobalBehaviour<Persistent> : SingleBehaviour<Persistent> where Persistent : SingleBehaviour<Persistent>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
}
