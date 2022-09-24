using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Serializable Fields

    public float Speed = 1;

    #endregion

    #region Global Variables

    private Vector3 location;

    private ResultsManager resultManager;

    public bool avoid { private get; set; }

    #endregion

    #region Unity Lifecycle

    IEnumerator Start()
    {
        yield return new WaitUntil(() =>
            resultManager = ResultsManager.Instance);
    }

    void Update()
    {
        if (avoid)
        {
            // when player is blocked by models and cannot move anywhere
            if (location == transform.position)
                resultManager.FinalizeReport();

            return;
        }
            
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * Speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * Time.deltaTime * Speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * Speed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * Speed);
        }

        location = transform.position;
    }

    private void OnCollisionExit(Collision collision)
    {
        // release the brake when it's clear(no models standing in the way).
        if (collision.gameObject.tag == "Model")
            avoid = false;
    }

    #endregion

    #region Callback Functions

    public void AvoidObstacle(Vector3 pushVector)
    {
        avoid = true;
        Vector3 pushDir = pushVector.normalized,
                avoidDir = Quaternion.AngleAxis(90, Vector3.up) * pushDir; // avoid obstacles by turning 90 degree ahead.

        avoidDir.Normalize();
        transform.Translate(avoidDir * Time.deltaTime * Speed);
        avoid = false;
    }

    #endregion

    
}
