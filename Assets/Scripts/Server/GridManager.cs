using Fusion;
using Unity.Burst;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Server
{
    public class GridManager : NetworkBehaviour, IPlayerJoined
    {
        [SerializeField] private GridData gridData;
        [SerializeField] private GameObject breakableBlockPrefab;
        [SerializeField] private Transform gridTransform;

        private bool _hadRenderedGrid;
        private BitmapGrid _grid;
        private GameObject[] _generatedBlocks;

        public override void Spawned()
        {
            if (Runner.IsServer)
            {
                GenerateGrid();
                ForceRenderGrid();
            }
        }

        public void PlayerJoined(PlayerRef player) => SendGridInformationRPC(player, _grid.GetBitmap());

        #region Grid Generation

        // ReSharper disable once UnusedMember.Global
        private void GenerateGrid()
        {
            _grid = new BitmapGrid(gridData.width, gridData.height);
            for (int x = 0; x < gridData.width; x++)
            {
                for (int z = 0; z < gridData.height; z++)
                {
                    if (gridData.IsSpawnZone(x, z)) continue;
                    if (!IsBreakableBlock(x, z)) continue;
                    _grid.AddObject(x, z);
                }
            }
        }

        private bool IsAdjacentToSpawnZone(int x, int z) =>
            // Adjacent to top-left corner (0,0)
            (x <= 1 && z <= 1 && !(x == 0 && z == 0)) ||
            // Adjacent to bottom-right corner (width-1, height-1)
            (x >= gridData.width - 2 && z >= gridData.height - 2 &&
             !(x == gridData.width - 1 && z == gridData.height - 1)) ||
            // Adjacent to bottom-left corner (0, height-1)
            (x <= 1 && z >= gridData.height - 2 && !(x == 0 && z == gridData.height - 1)) ||
            // Adjacent to top-right corner (width-1, 0)
            (x >= gridData.width - 2 && z <= 1 && !(x == gridData.width - 1 && z == 0));

        private bool IsBreakableBlock(int x, int z) =>
            !IsAdjacentToSpawnZone(x, z) && !gridData.IsSolidBlock(x, z) && !gridData.IsBorder(x, z) &&
            Random.value < 0.7;

        #endregion

        internal void DespawnGridItem(in Vector3 position)
        {
            (int x, int z) = gridData.WorldPositionToGridPosition(position);

            if (!_grid.HasObject(x, z))
            {
                return;
            }

            _grid.RemoveObject(x, z);
            RemoveRenderedGridItemRPC(x, z);
        }

        private void ForceRenderGrid() => ForceRenderGridRPC(_grid.GetBitmap());

        #region Outbound RPCs

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void ForceRenderGridRPC(uint[] bitmapGridData) => RenderGrid(bitmapGridData);

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void SendGridInformationRPC([RpcTarget] PlayerRef player, uint[] bitmapGridData) =>
            RenderGrid(bitmapGridData);

        private void RenderGrid(uint[] bitmapGridData)
        {
            if (_hadRenderedGrid)
            {
                Debug.Log("Grid already rendered, skipped");
                return;
            }

            Debug.Log(
                $"Rendering grid with: {bitmapGridData.Length}, {bitmapGridData[0]}, {bitmapGridData[1]}, {bitmapGridData[2]}");

            var bitmapGrid = new BitmapGrid(gridData.width, gridData.height, bitmapGridData);
            _generatedBlocks = new GameObject[gridData.width * gridData.height];

            float xOffset = (gridData.width - 1) * gridData.spacing * 0.5f;
            float zOffset = (gridData.height - 1) * gridData.spacing * 0.5f;
            for (int x = 0; x < gridData.width; x++)
            {
                for (int z = 0; z < gridData.height; z++)
                {
                    if (!bitmapGrid.HasObject(x, z))
                    {
                        continue;
                    }

                    var pos = gridData.startPosition + new Vector3(x * gridData.spacing - xOffset,
                        gridData.blockYOffset, z * gridData.spacing - zOffset);
                    var go = Instantiate(breakableBlockPrefab, pos, Quaternion.identity, gridTransform);

                    _generatedBlocks[gridData.GetLinearCoordinates(x, z)] = go;
                }
            }

            _hadRenderedGrid = true;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RemoveRenderedGridItemRPC(int x, int z)
        {
            int linearCoordinates = gridData.GetLinearCoordinates(x, z);
            Destroy(_generatedBlocks[linearCoordinates]);
            _generatedBlocks[linearCoordinates] = null;
        }

        #endregion
    }
}
