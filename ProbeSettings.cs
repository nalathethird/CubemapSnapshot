using UnityEngine;
using System;

namespace CubemapMaker
{
    [System.Serializable]
    public class ProbeSettings
    {
        // Runtime Settings
        public int importance;
        public float intensity;
        public bool boxProjection;
        public float blendDistance;
        public Vector3 boxSize;
        public Vector3 boxOffset;

        // Capture Settings
        public int resolution;
        public bool hdr;
        public float shadowDistance;
        public Color backgroundColor;
        public float nearClipPlane;
        public float farClipPlane;
    }
} 