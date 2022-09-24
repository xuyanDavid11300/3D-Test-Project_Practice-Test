using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameManager: MonoBehaviour
{
    #region Serializable Fields

    public List<ModelCapacities> capacities;

    public UnityEvent<PrimitiveType, float> OnModelCollected;

    #endregion

    #region Global Variables

    private float startTime; // current game start time(to calculate stretch factors for models).

    private int currentLevel;

    private bool sourceLoaded;

    private GameObject playZone;

    private Vector3 stageExtents;

    private LevelsManager levelManager;

    #endregion

    #region Properties

    private Color[] brushColors =
    {
        Color.red,
        Color.yellow,
        Color.green,
        Color.cyan,
        Color.magenta,
        Color.blue,
        Color.grey,
        Color.gray,
        Color.black
    };

    public Dictionary<PrimitiveType, ModelPool> ModelPools { get; private set; }

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        sourceLoaded = false;
        StartCoroutine(LoadSourcesCoroutine());
    }

    IEnumerator LoadSourcesCoroutine()
    {
        yield return new WaitUntil(() =>
        {
            levelManager = LevelsManager.Instance;            
            if (levelManager && levelManager.LoadCompleted)
            {
                currentLevel = levelManager.ActiveIndex + 1;
                //levelManager.LoadNewLevel(string.Format("Level {0} Scene",currentLevel));
                MeasureGameStage();
                LoadModelPools();

                sourceLoaded = true;
                return true;
            }

            return false;
        });
    }

    //// Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitUntil(() =>
        {
            if (levelManager
            && levelManager.LoadCompleted
            && sourceLoaded)
            {
                startTime = Time.time;
                ShowPoolModels();
                return true;
            }

            return false;
        });
    }

    #endregion

    #region Private Functions

    private void MeasureGameStage()
    {
        this.playZone = GameObject.FindGameObjectWithTag("Play Zone");
        if (this.playZone.TryGetComponent(out Renderer stageRenderer))
            this.stageExtents = Vector3.Scale(stageRenderer.bounds.extents, this.playZone.transform.localScale);
    }

    private void LoadModelPools()
    {
        if (capacities != null && capacities.Count > 0)
        {
            ModelPools = capacities.GroupBy(capacity => capacity.ModelType)
            .ToDictionary(model => model.Key,
                model =>
                {
                    var modelType = model.Key;
                    var capacity = model.FirstOrDefault();

                    if (capacity != null)
                    {
                        var primitiveCapacity = capacity.PrimitiveCapacity;
                        var modelPool = new ModelPool(primitiveCapacity, modelType, playZone.transform, levelManager.transform);
                        return modelPool;
                    }

                    return null;
                }
            );
        }
    }

    private void ShowPoolModels()
    {
        if (ModelPools != null && ModelPools.Count > 0)
        {
            foreach (var levelPool in ModelPools)
            {
                var modelType = levelPool.Key;
                var modelPool = levelPool.Value;

                var models = modelPool.GetAll();
                foreach (var model in models)
                    DecorateModel(model);
            }
        }
    }

    private void DecorateModel(GameObject model)
    {
        var delta = (Time.time - startTime) / 60f; // easy to calculate on time-scale.

        // start from 1 whichever level is on as the game stages are all scaled accordingly for each level.
        delta = 1 + UnityEngine.Random.Range(0, currentLevel * delta); 

        ResizeModel(model, delta);
        PlaceModel(model, delta);
        PaintModel(model);
        DropModel(model, delta);
        AttachBehaviours(model);
    }

    private void AttachBehaviours(GameObject model)
    {
        if (!model.TryGetComponent(out PushDetector pushDetector))
        {
            pushDetector = model.AddComponent<PushDetector>();
            if (pushDetector)
            {
                if (!model.TryGetComponent(out ModelDisposer modelDisposer))
                {
                    modelDisposer = model.AddComponent<ModelDisposer>();
                    if (modelDisposer)
                    {
                        //modelDisposer.ShowModel();
                        modelDisposer.OnRecycleModel = RecyleStageModel;
                        pushDetector.OnPushMoved = modelDisposer.DissolveModel;

                        var playerAvatar = GameObject.FindGameObjectWithTag("Player");
                        if (playerAvatar)
                        {
                            if (playerAvatar.TryGetComponent(out PlayerController playerController))
                                pushDetector.OnPushStill = playerController.AvoidObstacle;
                        }
                    }
                }
            }
        };
    }

    private void DropModel(GameObject model, float delta)
    {
        if (!model.TryGetComponent(out Rigidbody modelBody))
        {
            modelBody = model.AddComponent<Rigidbody>();
            if (modelBody)
            {
                modelBody.mass = currentLevel * delta; // mass cannot be scaled from a top-down structure. 
                modelBody.drag = 0;
                modelBody.angularDrag = 0;
                modelBody.useGravity = true;
                modelBody.isKinematic = false;
                modelBody.interpolation = RigidbodyInterpolation.Interpolate;
                modelBody.detectCollisions = true;
                modelBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                modelBody.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
    }

    private void PaintModel(GameObject model)
    {
        if (model.TryGetComponent(out Renderer renderer))
        {
            var material = renderer.material;
            if (material)
            {
                var colorIndex = UnityEngine.Random.Range(0, brushColors.Length);
                var brushColor = brushColors.ElementAtOrDefault(colorIndex);
                material.color = brushColor;
            }
        }
    }

    private void PlaceModel(GameObject model, float delta)
    {
        if (model.TryGetComponent(out Renderer renderer))
        {
            var bounds = renderer.bounds;
            if (bounds != null)
            {
                var extends = bounds.extents;
                if (extends != null)
                {
                    float y = extends.y * delta,
                          x = UnityEngine.Random.Range(-stageExtents.x, stageExtents.x),
                          z = UnityEngine.Random.Range(-stageExtents.z, stageExtents.z);

                    var dropSpot = new Vector3(x, y, z);
                    dropSpot /= currentLevel; // StageSize is measured in world scale, scale down to fit stage space. 
                    model.transform.localPosition = dropSpot;
                }
            }
        }
    }

    private void ResizeModel(GameObject model, float delta)
    {
        var scale = delta * Vector3.one;
        model.transform.localScale = scale;
    }

    public void RecyleStageModel(GameObject modelObject)
    {
        if (modelObject)
        {
            var collectMass = 0f;
            if (modelObject.TryGetComponent(out Rigidbody modelBody))
            {
                collectMass = modelBody.mass;
                Component.DestroyImmediate(modelBody);
            }
                
            if (modelObject.TryGetComponent(out PushDetector pushDetector))
                Component.DestroyImmediate(pushDetector);

            if (modelObject.TryGetComponent(out ModelDisposer modelDisposer))
                Component.DestroyImmediate(modelDisposer);

            var modelType = modelObject.name.Replace("Model", string.Empty);
            if (Enum.TryParse(modelType, out PrimitiveType enumType))
            {
                // recycle one model and pop another new one randomly on stage.
                if (ModelPools != null && ModelPools.TryGetValue(enumType, out ModelPool modelPool))
                {
                    modelPool.ReturnModelToPool(modelObject);
                    if (OnModelCollected != null)   
                        OnModelCollected.Invoke(enumType, collectMass);

                    var newModel = modelPool.GetPrimitiveModel();
                    if (newModel)
                        DecorateModel(newModel);
                }
            }
        }
    }

    #endregion
}
