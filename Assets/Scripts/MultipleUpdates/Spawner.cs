using UnityEngine;

namespace DefaultNamespace.MultipleUpdates
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _side;
        [SerializeField] private int _depth;
        [SerializeField] private float _yOffset;

        private void Start()
        {
            ExtensionMethods.SpawnLoop(_side, _depth, (i, pos) =>
            {
                Instantiate(_prefab, new Vector3(pos.x, pos.y * _yOffset, pos.z), Quaternion.identity, transform).AddComponent<CubeWithUpdate>();
            });
        }
    }
}