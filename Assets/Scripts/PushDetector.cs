using System;
using UnityEngine;

public class PushDetector : MonoBehaviour
{
    #region Global Fields

    public Action OnPushMoved;

    public Action<Vector3> OnPushStill;

    private bool pushed;

    private Vector3 pushForce;

    private Rigidbody pushBody;

    private Material pushMaterial;

    private PlayerController playerController;

    #endregion

    #region Properties

    private int levelMultiplier
    {
        get
        {
            var levelManager = LevelsManager.Instance;
            if (levelManager)
                return levelManager.ActiveIndex + 1;
            return 0;
        }
    }

    #endregion

    #region Unity Lifecycle

    void OnEnable()
    {
        if (TryGetComponent(out pushBody))
        {
            if (TryGetComponent(out Renderer pushRenderer))
                pushMaterial = pushRenderer.material;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (pushBody)
            {
                var pushVector = transform.position - other.transform.position;
                pushVector.y = 0; // we only care about xz movement.
                pushVector.Normalize();
                
                var power = 1.3f * Physics.gravity.magnitude * levelMultiplier; // the multiplier is tuned from practices.
                pushForce = power * pushVector;
                pushBody.AddForce(pushForce);

                pushed = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            pushed = false;
            if (other.TryGetComponent(out playerController))
                playerController.avoid = false; // remove player's brake when he isn't pushing.

            if (pushMaterial)
            {
                var emissive = pushMaterial.IsKeywordEnabled("_EMISSION");
                if (emissive)
                {
                    pushMaterial.SetColor("_EmissionColor", Color.clear);
                    pushMaterial.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (pushed)
        {
            if (pushBody)
            {
                if(pushBody.velocity.magnitude > .01f)
                    OnPushMoved.Invoke();
                
                else
                {
                    OnPushStill.Invoke(pushForce);
                    WarnObstacle();
                }
            }
        }        
    }

    #endregion

    #region Functions / Methods

    private void WarnObstacle()
    {
        if (pushMaterial)
        {
            var emissive = pushMaterial.IsKeywordEnabled("_EMISSION");
            if (!emissive)
            {
                pushMaterial.EnableKeyword("_EMISSION");

                var emissiveColor = pushMaterial.color;
                pushMaterial.SetColor("_EmissionColor", emissiveColor);
            }  
        }
    }

    #endregion
}
