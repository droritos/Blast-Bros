using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "New Grid Data Object", menuName = "Game/Grid Data")]
    public class GridData : ScriptableObject
    {
        [Header("Grid Data")]
        public Vector3 startPosition;
        public int width = 13;
        public int height = 11;

        [Header("Grid Offsets")]
        public float spacing = 1f;
        public float blockYOffset = 2f;

        // Cached values to avoid repeated calculations
        [SerializeField] [HideInInspector] private float cachedXOffset;
        [SerializeField] [HideInInspector] private float cachedZOffset;
        [SerializeField] [HideInInspector] private float inverseSpacing;

        private void OnValidate() => CacheOffsets();
        private void CacheOffsets()
        {
            cachedXOffset = (width - 1) * spacing * 0.5f;
            cachedZOffset = (height - 1) * spacing * 0.5f;
            inverseSpacing = 1f / spacing;
        }

        public int GetLinearCoordinates(int x, int z) => z * width + x;

        public Vector3 GridPositionToWorldPosition(int x, int z) =>
            new(
                startPosition.x + x * spacing - cachedXOffset,
                startPosition.y + blockYOffset,
                startPosition.z + z * spacing - cachedZOffset
            );

        public (int x, int z) WorldPositionToGridPosition(Vector3 worldPos)
        {
            // Calculate relative position and convert to grid coordinates in one step
            int x = Mathf.RoundToInt((worldPos.x - startPosition.x + cachedXOffset) * inverseSpacing);
            int z = Mathf.RoundToInt((worldPos.z - startPosition.z + cachedZOffset) * inverseSpacing);

            return (x, z);
        }

        public Vector3 AlignToClosestGridPosition(Vector3 worldPos)
        {
            var (x, z) = WorldPositionToGridPosition(worldPos);

            // Clamp to grid bounds
            x = Mathf.Clamp(x, 0, width - 1);
            z = Mathf.Clamp(z, 0, height - 1);

            return GridPositionToWorldPosition(x, z);
        }

        public bool IsBorder(int x, int z) => x == -1 || x == width || z == -1 || z == height;

        public bool IsSpawnZone(int x, int z) =>
            (x == 0 && z == 0) ||
            (x == width - 1 && z == height - 1) ||
            (x == 0 && z == height - 1) ||
            (x == width - 1 && z == 0);

        public bool IsSolidBlock(int x, int z) => x % 2 == 1 && z % 2 == 1;
    }
}
