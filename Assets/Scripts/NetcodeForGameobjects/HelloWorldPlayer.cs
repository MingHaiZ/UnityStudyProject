using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NetcodeForGameobjects
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Move();
            }
        }

        public void Move()
        {
            SubmitPositionRequestRpc();
        }

        [Rpc(SendTo.Server)]
        private void SubmitPositionRequestRpc(RpcParams rpcParams = default)
        {
            var randomPosition = GetRandormPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }

        private Vector3 GetRandormPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        private void Update()
        {
            if (transform.position != Position.Value)
            {
                transform.position = Position.Value;
            }
        }
    }
}