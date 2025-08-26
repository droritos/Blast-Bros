using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Server
{
    public class GridGenerator : MonoBehaviour
    {
        [SerializeField] private GridData gridData;
        [SerializeField] private GameObject breakableBlockPrefab;
        [SerializeField] private Transform gridTransform;

        // ReSharper disable once UnusedMember.Global
        public void GenerateGrid()
        {
            ClearGridContents();
            NetworkRunner runner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());

            // precalc some values
            float xOffset = (gridData.width - 1) * gridData.spacing * 0.5f;
            float zOffset = (gridData.height - 1) * gridData.spacing * 0.5f;
            for (int x = 0; x < gridData.width; x++)
            {
                for (int z = 0; z < gridData.height; z++)
                {
                    if (gridData.IsSpawnZone(x, z)) continue;
                    if (IsBreakableBlock(x, z))
                    {
                        var pos = gridData.startPosition + new Vector3(x * gridData.spacing - xOffset,
                            gridData.blockYOffset, z * gridData.spacing - zOffset);
                        var go = runner.Spawn(breakableBlockPrefab, pos, Quaternion.identity);
                        go.transform.SetParent(gridTransform);
                    }
                }
            }
        }

        private void ClearGridContents()
        {
            for (int i = gridTransform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(gridTransform.GetChild(i).gameObject);
            }
        }

        private bool IsAdjacentToSpawnZone(int x, int z) =>
            // Adjacent to top-left corner (0,0)
            (x <= 1 && z <= 1 && !(x == 0 && z == 0)) ||
            // Adjacent to bottom-right corner (width-1, height-1)
            (x >= gridData.width - 2 && z >= gridData.height - 2 && !(x == gridData.width - 1 && z == gridData.height - 1)) ||
            // Adjacent to bottom-left corner (0, height-1)
            (x <= 1 && z >= gridData.height - 2 && !(x == 0 && z == gridData.height - 1)) ||
            // Adjacent to top-right corner (width-1, 0)
            (x >= gridData.width - 2 && z <= 1 && !(x == gridData.width - 1 && z == 0));

        private bool IsBreakableBlock(int x, int z) =>
            !IsAdjacentToSpawnZone(x, z) && !gridData.IsSolidBlock(x, z) && !gridData.IsBorder(x, z) && Random.value < 0.7;
    }
}
