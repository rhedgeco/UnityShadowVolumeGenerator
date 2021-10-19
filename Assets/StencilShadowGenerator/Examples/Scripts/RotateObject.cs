using UnityEngine;

namespace StencilShadowGenerator.Examples.BasicScene.Scripts
{
    public class RotateObject : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationSpeed = Vector3.zero;
        
        private void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
