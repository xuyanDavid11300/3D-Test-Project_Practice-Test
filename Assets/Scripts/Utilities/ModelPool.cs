using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelPool
{
    #region Global Fields

    private readonly int capacity;

    private Transform stageForm,
                       poolForm;

    private PrimitiveType primeType;

    private Stack<GameObject> poolObjects = new Stack<GameObject>();

    private List<GameObject> playObjects = new List<GameObject>();

    #endregion

    #region Constructor

    public ModelPool(int capacity, PrimitiveType primeType, Transform stageForm, Transform poolForm)
    {
        this.capacity = capacity;
        this.primeType = primeType;
        this.stageForm = stageForm;
        this.poolForm = poolForm;

        InstantiatePrimitive();
    }

    #endregion

    #region Functions / Methods

    private void InstantiatePrimitive()
    {
        for (int i = 0; i < capacity; i++)
            CreatePrimitive();
    }

    private void CreatePrimitive()
    {
        var primeObject = GameObject.CreatePrimitive(primeType);
        primeObject.tag = "Model";
        ReturnModelToPool(primeObject);
    }

    /// <summary>
    /// Returns instance of prefab.
    /// </summary>
    /// <returns>Instance of prefab.</returns>
    public GameObject GetPrimitiveModel()
    {
        GameObject model = null;
        var totalCount = poolObjects.Count + playObjects.Count;

        // if we dont have enough primitives create new instances of them
        if (totalCount < capacity)
        {
            for (int i = totalCount; i <= capacity; i++)
                CreatePrimitive();

            model = GetPrimitiveModel();
        }
        // otherwise we can get them from pool
        else
        {
            // get object from pool
            model = poolObjects.Pop();

            var instanceName = model.name;
            if (instanceName.EndsWith("Instance"))
                instanceName = instanceName.Replace("Instance", "Model");
            else
                instanceName += "Model";

            model.name = instanceName;

            // remove parent
            model.transform.SetParent(stageForm);

            // reset position
            model.transform.localPosition = Vector3.zero;
            model.transform.localScale = Vector3.one;
            model.transform.localEulerAngles = Vector3.one;

            // activate object
            model.gameObject.SetActive(true);
            playObjects.Add(model);
        }

        return model;
    }

    public List<GameObject> GetAll()
    {
        while (poolObjects.Count > 0)
            GetPrimitiveModel();

        return playObjects;
    }

    /// <summary>
    /// Returns instance to the pool.
    /// </summary>
    /// <param name="modelObject">Prefab instance.</param>
    public void ReturnModelToPool(GameObject modelObject)
    {
        var instanceName = modelObject.name;
        if (instanceName.EndsWith("Model"))
            instanceName = instanceName.Replace("Model", "Instance");
        else
            instanceName += "Instance";

        modelObject.name = instanceName;

        // disable object
        modelObject.gameObject.SetActive(false);
        // set parent as this object
        modelObject.transform.SetParent(poolForm.transform);
        // reset position
        modelObject.transform.localPosition = Vector3.zero;
        modelObject.transform.localScale = Vector3.one;
        modelObject.transform.localEulerAngles = Vector3.one;

        // add to pool
        if (playObjects.Contains(modelObject))
            playObjects.Remove(modelObject);

        poolObjects.Push(modelObject);
    }

    public void RecycleAll()
    {
        while (playObjects.Count > 0)
        {
            var lastObject = playObjects.LastOrDefault();
            if (lastObject)
                ReturnModelToPool(lastObject);
        }
    }

    #endregion
}
