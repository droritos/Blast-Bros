using System;
using UnityEngine;

namespace Game.Server
{
    // Non-networked version for local operations and backwards compatibility
    internal class BitmapGrid
    {
        private readonly int width;
        private readonly int height;
        private readonly uint[] bitmap;
        private readonly int bitsPerElement = 32; // using uint (32-bit)

        public BitmapGrid(int width, int height)
        {
            this.width = width;
            this.height = height;

            // Calculate how many uint elements we need to store all cells
            int totalCells = width * height;
            int elementsNeeded = (totalCells + bitsPerElement - 1) / bitsPerElement; // Ceiling division
            bitmap = new uint[elementsNeeded];
        }

        public BitmapGrid(int width, int height, uint[] bitmap)
        {
            this.width = width;
            this.height = height;
            this.bitmap = bitmap;
        }

        /// <summary>
        /// Converts 2D coordinates to a linear bit index
        /// </summary>
        private int GetBitIndex(int x, int z)
        {
            if (x < 0 || x >= width || z < 0 || z >= height)
                throw new ArgumentOutOfRangeException(
                    $"Coordinates ({x}, {z}) are out of bounds for grid size ({width}, {height})");

            return z * width + x;
        }

        /// <summary>
        /// Gets the array index and bit position for a given coordinate
        /// </summary>
        private void GetArrayIndexAndBitPosition(int x, int z, out int arrayIndex, out int bitPosition)
        {
            int bitIndex = GetBitIndex(x, z);
            arrayIndex = bitIndex / bitsPerElement;
            bitPosition = bitIndex % bitsPerElement;
        }

        public uint[] GetBitmap() => bitmap;

        public bool HasObject(int x, int z)
        {
            GetArrayIndexAndBitPosition(x, z, out int arrayIndex, out int bitPosition);
            return (bitmap[arrayIndex] & (1u << bitPosition)) != 0;
        }

        public void AddObject(int x, int z)
        {
            GetArrayIndexAndBitPosition(x, z, out int arrayIndex, out int bitPosition);
            bitmap[arrayIndex] |= 1u << bitPosition;
        }

        public void RemoveObject(int x, int z)
        {
            GetArrayIndexAndBitPosition(x, z, out int arrayIndex, out int bitPosition);
            bitmap[arrayIndex] &= ~(1u << bitPosition);
        }

        public void Clear() => Array.Clear(bitmap, 0, bitmap.Length);
    }
}
