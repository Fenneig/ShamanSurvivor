#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unity.Entities.HybridViews.Modules.HybridEntityViews
{
    /// <summary>
    /// Draws an editor-only preview of the <see cref="EntityView"/> assigned to an entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component renders the meshes of the referenced
    /// <see cref="EntityView"/> directly in the Scene view without creating a
    /// runtime view instance.
    /// </para>
    ///
    /// <para>
    /// The preview is rendered only while editing a scene. It is disabled during
    /// Play Mode and while editing a prefab in Prefab Mode.
    /// </para>
    ///
    /// <para>
    /// Both static and skinned meshes are supported. Skinned meshes are baked
    /// into a temporary mesh before being rendered.
    /// </para>
    /// </remarks>
    [ExecuteAlways]
    [RequireComponent(typeof(EntityViewPrefabAuthoring))]
    public class EntityPreview : MonoBehaviour
    {
        private EntityViewPrefabAuthoring _authoring;
        private Mesh _bakedMesh;

        private Mesh BakedMesh
        {
            get
            {
                return _bakedMesh ??= new Mesh
                {
                    name = $"{nameof(EntityPreview)}_Baked"
                };
            }
        }

        private void OnEnable()
        {
            TryGetComponent(out _authoring);
        }

        private void OnDisable()
        {
            if (_bakedMesh == null)
                return;

            DestroyImmediate(_bakedMesh);
            _bakedMesh = null;
        }

        private void OnRenderObject()
        {
            if (!CanRenderPreview())
                return;

            _authoring ??= GetComponent<EntityViewPrefabAuthoring>();

            if (_authoring.Value == null)
                return;

            DrawMeshes(_authoring.Value.transform, transform.localToWorldMatrix);
        }

        private bool CanRenderPreview() =>
            !Application.isPlaying &&
            isActiveAndEnabled &&
            gameObject.scene.IsValid() &&
            PrefabStageUtility.GetCurrentPrefabStage() == null &&
            Camera.current is
            {
                cameraType: CameraType.SceneView
            };

        private void DrawMeshes(Transform root, Matrix4x4 rootMatrix)
        {
            Matrix4x4 rootInverse = root.worldToLocalMatrix;

            foreach (MeshFilter meshFilter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!meshFilter.TryGetComponent(out MeshRenderer renderer))
                    continue;

                if (meshFilter.sharedMesh == null)
                    continue;

                DrawMesh(
                    meshFilter.sharedMesh,
                    GetMaterials(renderer),
                    rootMatrix *
                    rootInverse *
                    meshFilter.transform.localToWorldMatrix
                );
            }

            foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer.sharedMesh == null)
                    continue;

                renderer.BakeMesh(BakedMesh);

                DrawMesh(
                    BakedMesh,
                    GetMaterials(renderer),
                    rootMatrix *
                    rootInverse *
                    renderer.transform.localToWorldMatrix
                );
            }
        }

        private static void DrawMesh(Mesh mesh, Material[] materials, Matrix4x4 matrix)
        {
            int subMeshCount = Mathf.Min(mesh.subMeshCount, materials.Length);

            for (int i = 0; i < subMeshCount; i++)
            {
                Material material = materials[i];

                if (material == null)
                    continue;

                material.SetPass(0);
                Graphics.DrawMeshNow(mesh, matrix, i);
            }
        }

        /// <summary>
        /// Returns the materials used to render the specified <see cref="Renderer"/>.
        /// </summary>
        /// <remarks>
        /// Override this method to customize the materials used for the editor
        /// preview. By default, the renderer's shared materials are returned.
        /// </remarks>
        /// <param name="renderer">
        /// The renderer being previewed.
        /// </param>
        /// <returns>
        /// The materials used for preview rendering.
        /// </returns>
        protected virtual Material[] GetMaterials(Renderer renderer)
        {
            return renderer.sharedMaterials;
        }
    }
}
#endif