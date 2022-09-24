using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Model Recycling Component.
/// </summary>
public class ModelDisposer : MonoBehaviour
{
    #region Global Fields

    private Material modelMaterial;

    public Action<GameObject> OnRecycleModel;

    #endregion

    #region Unity Lifecycle

    void OnEnable()
    {
        if (TryGetComponent(out Renderer renderer))
        {
            // Add Fading Effect.
            modelMaterial = renderer.material;
            if (modelMaterial)
                modelMaterial.ToFadeMode();
        }
    }

    void OnDisable()
    {
        // Restore to Opaque Mode for next use.
        if (modelMaterial)
            modelMaterial.ToOpaqueMode();
    }

    #endregion

    #region Callback Functions

    public void DissolveModel()
    {
        if (modelMaterial)
            StartCoroutine(DiminishCoroutine());
    }

    IEnumerator DiminishCoroutine()
    {
        if (modelMaterial)
        {
            yield return StartCoroutine(modelMaterial.FadeTo(0, 2));
            yield return StartCoroutine(RecycleCoroutine());
        }
    }

    IEnumerator RecycleCoroutine()
    {
        yield return new WaitForEndOfFrame();
        if (OnRecycleModel != null)
            OnRecycleModel(gameObject);
    }

    #endregion
}
