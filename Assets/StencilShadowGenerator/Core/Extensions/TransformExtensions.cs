using UnityEngine;

namespace StencilShadowGenerator.Core.Extensions
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Resets a transforms local space
        /// </summary>
        public static void LocalReset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}