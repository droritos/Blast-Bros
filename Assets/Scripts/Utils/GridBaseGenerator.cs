using UnityEngine;

namespace Game
{
    [ExecuteInEditMode]
    public class GridBaseGenerator : MonoBehaviour
    {
        [Header("Grid Blocks")] //
        [SerializeField] private GameObject solidBlockPrefab;
        [SerializeField] private GameObject breakableBlockPrefab;
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject borderBlockPrefab;
        [SerializeField] private GridData gridData;

        [Header("Grid Objects")]
        [SerializeField] private Transform gridCollidersTransform;
        [SerializeField] private Transform spawnPositionsTransform;
        [SerializeField] private Transform gridBaseTransform;

        // ReSharper disable once UnusedMember.Global
        public void GenerateGridBase()
        {
            ClearGridBase();

            // precalc some values
            float xOffset = (gridData.width - 1) * gridData.spacing * 0.5f;
            float zOffset = (gridData.height - 1) * gridData.spacing * 0.5f;
            var upOffset = Vector3.up * gridData.blockYOffset;

            for (int x = -1; x < gridData.width + 1; x++)
            {
                for (int z = -1; z < gridData.height + 1; z++)
                {
                    var pos = gridData.startPosition + new Vector3(x * gridData.spacing - xOffset, z * 0,
                        z * gridData.spacing - zOffset);

                    // create floor tile
                    InstantiatePrefabInstance(floorPrefab, pos, Quaternion.identity, gridBaseTransform);

                    // create spawn zone markers and skip blocks
                    pos += upOffset;
                    if (gridData.IsSpawnZone(x, z))
                    {
                        var positionMarker = new GameObject("Spawn Marker")
                        {
                            transform =
                            {
                                parent = spawnPositionsTransform, position = pos, rotation = Quaternion.identity
                            }
                        };
                        continue;
                    }

                    // create block
                    if (gridData.IsBorder(x, z))
                    {
                        InstantiatePrefabInstance(borderBlockPrefab, pos, Quaternion.identity, gridBaseTransform);
                    }
                    else if (gridData.IsSolidBlock(x, z))
                    {
                        InstantiatePrefabInstance(solidBlockPrefab, pos, Quaternion.identity, gridBaseTransform);
                    }
                }
            }

            SpawnColliders();
        }

        private void SpawnColliders()
        {
            int paddedWidth = gridData.width + 2;
            int paddedHeight = gridData.height + 2;
            float spacing = gridData.spacing;

            float gridRightBorderX = paddedWidth - 0.5f * spacing;
            float gridTopBorderZ = paddedHeight - 0.5f * spacing;
            var verticalColliderSize = new Vector3(spacing, spacing, paddedHeight * spacing);
            var horizontalColliderSize = new Vector3(paddedWidth * spacing, spacing, spacing);

            // spawn floor collider
            SpawnCollider("Floor", gridData.startPosition,
                new Vector3((gridData.width + spacing) * spacing, spacing, (gridData.height + spacing) * spacing));

            // spawn floor collider
            SpawnCollider("Left", gridData.startPosition + new Vector3(-gridRightBorderX, gridData.blockYOffset, 0),
                verticalColliderSize);
            SpawnCollider("Right", gridData.startPosition + new Vector3(gridRightBorderX, gridData.blockYOffset, 0),
                verticalColliderSize);
            SpawnCollider("Top", gridData.startPosition + new Vector3(0, gridData.blockYOffset, gridTopBorderZ),
                horizontalColliderSize);
            SpawnCollider("Bottom", gridData.startPosition + new Vector3(0, gridData.blockYOffset, -gridTopBorderZ),
                horizontalColliderSize);
        }

        private void SpawnCollider(string colliderName, Vector3 position, Vector3 scale)
        {
            var colliderGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            colliderGameObject.name = colliderName;
            colliderGameObject.transform.SetParent(gridCollidersTransform);
            colliderGameObject.transform.position = position;
            colliderGameObject.transform.localScale = scale;
            DestroyImmediate(colliderGameObject.GetComponent<MeshFilter>());
            DestroyImmediate(colliderGameObject.GetComponent<MeshRenderer>());
        }

        public void ClearGridBase()
        {
            for (int i = gridCollidersTransform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(gridCollidersTransform.GetChild(i).gameObject);
            }

            for (int i = gridBaseTransform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(gridBaseTransform.GetChild(i).gameObject);
            }

            for (int i = spawnPositionsTransform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(spawnPositionsTransform.GetChild(i).gameObject);
            }
        }

        private GameObject InstantiatePrefabInstance(GameObject original, Vector3 position, Quaternion rotation,
            Transform parent)
        {
#if UNITY_EDITOR
            var instance = Application.isPlaying
                ? Instantiate(original)
                : UnityEditor.PrefabUtility.InstantiatePrefab(original) as GameObject;
#else
        var instance = Instantiate(original);
#endif

            var instanceTransform = instance.transform;
            instanceTransform.SetParent(parent);
            instanceTransform.SetPositionAndRotation(position, rotation);

            return instance;
        }

        // ReSharper disable once UnusedMember.Global
        public void SetCurrentPositionToStartPosition() => gridData.startPosition = transform.position;
    }
}
