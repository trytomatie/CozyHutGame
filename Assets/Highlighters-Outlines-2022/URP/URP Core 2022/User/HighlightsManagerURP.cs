using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

using System.Collections.Generic;

using Highlighters;

namespace Highlighters_URP
{
    public enum RenderingType
    {
        PerObject, PerObjectBlending
    }

    /// <summary>
    /// The core of highlighters asset.
    /// This script is a renderer feature that needs to be added to urp custom renderer data.
    /// It automatically detects all instanced of Highlihter class in the scene, based on that information,
    /// it creates required passes to draw outlines and overalys.
    /// </summary>
    public class HighlightsManagerURP : ScriptableRendererFeature
    {
        [SerializeField] public LayerMask DepthLayerMask;

        public enum RenderEvent
        {
            BeforeRenderingTransparents, AfterRenderingTransparents
        }

        [SerializeField] private RenderEvent renderingEvent;
        private RenderPassEvent renderPassEvent
        {
            get
            {
                switch (renderingEvent)
                {
                    case RenderEvent.BeforeRenderingTransparents:
                        return RenderPassEvent.BeforeRenderingTransparents;
                    case RenderEvent.AfterRenderingTransparents:
                        return RenderPassEvent.AfterRenderingTransparents;
                    default:
                        return RenderPassEvent.AfterRenderingTransparents;
                }
            }
        }

        /// <summary>
        /// Contains all highlighters in the scene. 
        /// </summary>
        public Dictionary<int, Highlighter> highlightersInScene = new Dictionary<int, Highlighter>();

        // Depth mask
        //private Dictionary<int, DepthMaskPass> depthMaskPasses = new Dictionary<int, DepthMaskPass>();

        private DepthMaskPass depthMaskPass;
        private bool renderSceneDepth = false;

        // Overlays
        private Dictionary<int, ObjectsPass> objectsPasses = new Dictionary<int, ObjectsPass>();
        private Dictionary<int, MeshOutlinePass> meshOutlinePasses = new Dictionary<int, MeshOutlinePass>();
        private Dictionary<int, OverlayPass> overlayPasses = new Dictionary<int, OverlayPass>();

        // Blurs
        private Dictionary<int, PerObjectBlurPass> perObjectBlurPasses = new Dictionary<int, PerObjectBlurPass>();

        // When mulitple highlight objects are activated at the same time, this bool ensures that only one instance of a function is called at the time
        // TODO: come up with a better solution
        // Think what could be done to call only one time HighlightersReset() when multiple highlighters are activated at the same time.
        private bool isWorking = false;


        private void OnEnable()
        {
            Highlighter.OnHighlighterValidate += HighlighterDataUpdate;
            Highlighter.OnHighlighterReset += HighlightersReset;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.projectChanged += ProjectChanged;
            EditorSceneManager.sceneSaved += SceneSaved;
#endif
        }

        private void OnDisable()
        {
            Highlighter.OnHighlighterValidate -= HighlighterDataUpdate;
            Highlighter.OnHighlighterReset -= HighlightersReset;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.projectChanged -= ProjectChanged;
            EditorSceneManager.sceneSaved -= SceneSaved;
#endif

        }

#if UNITY_EDITOR
        private void SceneSaved(Scene scene)
        {
            HighlightersReset();
        }

        private void ProjectChanged()
        {
            HighlightersReset();
        }
#endif

        /// <summary>
        /// Updates data in higlights manager when settings of highlighter are changed. Currently only in editor. 
        /// </summary>
        /// <param name="ID"></param>
        private void HighlighterDataUpdate(int ID)
        {
            if (!highlightersInScene.ContainsKey(ID) || ID == -1 || ID > highlightersInScene.Count)
            {
                HighlightersReset();
                return;
            }

            var highlighter = highlightersInScene[ID];

            if (highlighter.Settings.DepthMask != DepthMask.Disable) renderSceneDepth = true;

            var highlighterSettings = highlighter.Settings;

            var renderers = highlighter.Renderers;

            // This is not needed here, but it makes sure nothing breaks
            depthMaskPass = new DepthMaskPass(renderPassEvent, DepthLayerMask, "_SceneDepthMask");

            objectsPasses[ID] = new ObjectsPass(renderPassEvent, highlighterSettings, ID, "_ObjectsPass_" + ID.ToString(), renderers);
            var objectsInfo = objectsPasses[ID].objectsInfo;

            meshOutlinePasses[ID] = new MeshOutlinePass(renderPassEvent, highlighterSettings, ID, "_MeshOutlinePass_" + ID.ToString(), renderers);

            if (highlighterSettings.DepthMask != DepthMask.Disable)
            {
                // Not sure when RenderTargetIdentifier resets its values so better keep it here
                var sceneDepthMask = depthMaskPass.sceneDepthMask;
                objectsPasses[ID].SetupSceneDepthTarget(sceneDepthMask);
                meshOutlinePasses[ID].SetupSceneDepthTarget(sceneDepthMask);
            }

            overlayPasses[ID] = new OverlayPass(renderPassEvent, highlighterSettings, "_OverlayPass_" + ID.ToString());
            overlayPasses[ID].SetupObjectsTarget(objectsInfo);

            if (highlighterSettings.UseMeshOutline)
            {
                var meshOutlineObjects = meshOutlinePasses[ID].meshOutlineObjects;
                overlayPasses[ID].SetupMeshOutlineTarget(meshOutlineObjects);
            }

            perObjectBlurPasses[ID] = new PerObjectBlurPass(renderPassEvent, highlighterSettings, ID, "_PerObjectBlurPass_" + ID.ToString());
            perObjectBlurPasses[ID].SetupObjectsTarget(objectsInfo);
        }


        /// <summary>
        /// Fills highlightersInScene with highlighters from the scene and creates Id for each of the highlighters.
        /// </summary>
        private void GetAllSceneHighlighters()
        {
            highlightersInScene = new Dictionary<int, Highlighter>();

            var list = new List<Highlighter>(FindObjectsOfType<Highlighter>());

            foreach (var item in list)
            {
                item.Setup();
            }

            renderSceneDepth = false;

            foreach (var item in list)
            {
                if (item.enabled)
                {
                    if (item.ID != -1 && !highlightersInScene.ContainsKey(item.ID))
                    {
                        highlightersInScene.Add(item.ID, item);
                        if (item.Settings.DepthMask != DepthMask.Disable) renderSceneDepth = true;
                    }
                    else
                    {
                        // If Ids overlap, reset highlighters
                        item.Setup();
                        HighlightersReset();
                        return;
                    }
                }
            }
        }


        /// <summary>
        /// Called when highlighters need a reset.
        /// </summary>
        public void HighlightersReset()
        {
            // Activation multiple highlighters at the same time makes multiple HighlightersReset() funcitons work at the same time which results in dictionaries not being cleared here.
            if (isWorking) return;
            isWorking = true;

            // Scene Depth Mask
            depthMaskPass = new DepthMaskPass(renderPassEvent, DepthLayerMask, "_SceneDepthMask");

            // Overlay
            objectsPasses.Clear();
            meshOutlinePasses.Clear();
            overlayPasses.Clear();

            // Blur

            // VERY IMPORTANT
            // Releases allocated render textures
            foreach (var item in perObjectBlurPasses)
            {
                item.Value.Dispose();
            }

            perObjectBlurPasses.Clear();

            GetAllSceneHighlighters();

            foreach (var item in highlightersInScene)
            {
                var highlighter = item.Value;
                var ID = highlighter.ID;
                var highlighterSettings = highlighter.Settings;
                var renderers = highlighter.Renderers;

                objectsPasses.Add(ID, new ObjectsPass(renderPassEvent, highlighterSettings, ID, "_ObjectPass_" + ID.ToString(), renderers));
                var objectsInfo = objectsPasses[ID].objectsInfo;

                meshOutlinePasses.Add(ID, new MeshOutlinePass(renderPassEvent, highlighterSettings, ID, "_MeshOutlinePass_" + ID.ToString(), renderers));

                if (highlighterSettings.DepthMask != DepthMask.Disable)
                {
                    var sceneDepthMask = depthMaskPass.sceneDepthMask;
                    objectsPasses[ID].SetupSceneDepthTarget(sceneDepthMask);
                    meshOutlinePasses[ID].SetupSceneDepthTarget(sceneDepthMask);
                }

                overlayPasses.Add(ID, new OverlayPass(renderPassEvent, highlighterSettings, "_OverlayPass_" + ID.ToString()));
                overlayPasses[ID].SetupObjectsTarget(objectsInfo);

                if (highlighterSettings.UseMeshOutline)
                {
                    var meshOutlineObjects = meshOutlinePasses[ID].meshOutlineObjects;
                    overlayPasses[ID].SetupMeshOutlineTarget(meshOutlineObjects);
                }

                // Blur
                perObjectBlurPasses.Add(ID, new PerObjectBlurPass(renderPassEvent, highlighterSettings, ID, "_PerObjectBlur_" + ID.ToString()));
                perObjectBlurPasses[ID].SetupObjectsTarget(objectsInfo);

            }

            isWorking = false;
        }

        // Called when:
        // When the Renderer Feature loads the first time.
        // When you enable or disable the Renderer Feature.
        // When you change a property in the inspector of the Renderer Feature.
        public override void Create()
        {
            HighlightersReset();
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            // Overlays setup
            foreach (var highlighter in highlightersInScene)
            {
                int ID = highlighter.Key;

                if (!highlightersInScene.ContainsKey(ID) || !objectsPasses.ContainsKey(ID)) //  || ID == -1 || ID > highlightersInScene.Count
                {
                    HighlightersReset();
                    return;
                }

                // Per object blur passes
                if (highlightersInScene[ID].Settings.UseOuterGlow)
                {
                    perObjectBlurPasses[ID].ConfigureInput(ScriptableRenderPassInput.Color);
                    perObjectBlurPasses[ID].SetCameraTarget(renderer.cameraColorTargetHandle);
                }

                // Overlay passes
                if (highlightersInScene[ID].Settings.IsAnyOverlayUsed())
                {
                    overlayPasses[ID].ConfigureInput(ScriptableRenderPassInput.Color);
                    overlayPasses[ID].SetCameraTarget(renderer.cameraColorTargetHandle);
                }

            }
        }

        // Unity calls this method every frame, once for each Camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView) return;

            // Scene Depth Mask
            if (renderSceneDepth) renderer.EnqueuePass(depthMaskPass);

            // Overlays setup
            foreach (var highlighter in highlightersInScene)
            {
                int ID = highlighter.Key;

                if (!highlightersInScene.ContainsKey(ID) || !objectsPasses.ContainsKey(ID)) //  || ID == -1 || ID > highlightersInScene.Count
                {
                    HighlightersReset();
                    return;
                }

                // Objects info passes
                renderer.EnqueuePass(objectsPasses[ID]);

                // Mesh outline passes
                if (highlightersInScene[ID].Settings.UseMeshOutline) renderer.EnqueuePass(meshOutlinePasses[ID]);

                // Per object blur passes
                if (highlightersInScene[ID].Settings.UseOuterGlow)
                {
                    renderer.EnqueuePass(perObjectBlurPasses[ID]);
                }

                // Overlay passes
                if (highlightersInScene[ID].Settings.IsAnyOverlayUsed())
                {
                    //overlayPasses[ID].SetCameraTarget(renderer.cameraColorTargetHandle);
                    renderer.EnqueuePass(overlayPasses[ID]);
                }

            }
        }

        protected override void Dispose(bool disposing)
        {
            // Use this funciton in future to ensure no materials leaks /or no render texture leaks
        }

    }
}
