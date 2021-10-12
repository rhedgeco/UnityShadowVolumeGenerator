using UnityEngine;

namespace StencilShadowGenerator.Core.Extensions
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Naive way of resetting transform coordinates
        /// Breaks down when in a nested structure with different scaling
        /// </summary>
        /// <param name="transform"></param>
        public static void Reset(this Transform transform)
        {
            // TODO: make work with nested structure with different scaling
            // this would require a chain lookup to the root object, undoing all transforms
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}