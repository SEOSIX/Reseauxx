using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private Mesh[] availableMeshes;


    private GameObject lastHitObject;

    private NetworkVariable<PlayerData> playerData = new(
        new PlayerData { life = 100, stunt = false },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    private NetworkVariable<Color> playerColor = new(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Renderer _renderer;
    private Vector3 direction;

    public override void OnNetworkSpawn()
    {
        _renderer = GetComponentInChildren<Renderer>();
        playerColor.OnValueChanged += (oldColor, newColor) =>
        {
            _renderer.material.color = newColor;
        };

        if (IsServer)
        {
            if (IsHost)
                playerColor.Value = Color.green;
            if (OwnerClientId == 1)
                playerColor.Value = Color.blue;
        }

        _renderer.material.color = playerColor.Value;

        if (IsOwner)
        {
            FollowCamera cam = Camera.main.GetComponent<FollowCamera>();
            if (cam != null)
            {
                cam.SetTarget(transform);
            }
        }
    }

    void Update()
    {
        if (!IsOwner)
            return;
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        direction = new Vector3(moveX, 0f, moveY).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDir = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * direction;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            transform.forward = moveDir;
        }
        Vector3 cameraForward = Camera.main.transform.forward;
        Debug.DrawRay(Camera.main.transform.position, cameraForward * 7, Color.green);

        if (Physics.Raycast(Camera.main.transform.position, cameraForward, out RaycastHit hit, 3))
        {
            if (hit.collider.CompareTag("ObjectToTransform"))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject != lastHitObject)
                {
                    if (lastHitObject != null)
                        lastHitObject.GetComponent<Renderer>().material.color = Color.white;

                    hitObject.GetComponent<Renderer>().material.color = Color.cyan;
                    lastHitObject = hitObject;
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    var hitNetworkObject = hit.collider.GetComponent<NetworkObject>();
                    if (hitNetworkObject != null)
                        ChangeMeshServerRpc(hitNetworkObject.NetworkObjectId);
                }
            }
            CatchHidder(hit);
        }
        else
        {
            if (lastHitObject != null)
            {
                lastHitObject.GetComponent<Renderer>().material.color = Color.white;
                lastHitObject = null;
            }
        }
    }


    #region BaseForProptHunt

    

    [ServerRpc]
    void ChangeMeshServerRpc(ulong targetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject targetObject))
        {
            
            Vector3 targetScale = targetObject.transform.localScale;
            Mesh mesh = targetObject.GetComponentInChildren<MeshFilter>().mesh;
            Collider targetCollider = targetObject.GetComponentInChildren<Collider>();
            
            ApplyMeshRpc(mesh);
            ChangeMeshClientRpc(targetId,targetScale );
            CopyCollider(targetCollider);
        }
    }

    [ClientRpc]
    void ChangeMeshClientRpc(ulong targetId, Vector3 targetScale)
    {
        if (!IsOwner && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject targetObject))
        {
            Mesh mesh = targetObject.GetComponentInChildren<MeshFilter>().mesh;
            Collider targetCollider = targetObject.GetComponentInChildren<Collider>();
            ApplyMeshRpc(mesh);
            CopyCollider(targetCollider);
        }
    }
    private void ApplyMeshRpc(Mesh mesh)
    {
        if (mesh != null)
            GetComponentInChildren<MeshFilter>().mesh = mesh;
    }
    
    
    [ServerRpc(RequireOwnership = false)]
    public void SetRoleServerRpc(string newTag)
    {
        gameObject.tag = newTag;
        SetRoleClientRpc(newTag);
    }

    [ClientRpc]
    private void SetRoleClientRpc(string newTag)
    {
        gameObject.tag = newTag;
    }
    private void CopyCollider(Collider sourceCollider) 
    {
        foreach (Collider oldCol in GetComponentsInChildren<Collider>())
        {
            Destroy(oldCol);
        }

        if (sourceCollider == null)
        {
            Debug.LogWarning("Aucun collider à copier !");
            return;
        }
        Transform targetParent = GetComponentInChildren<MeshRenderer>()?.transform ?? transform;
        if (sourceCollider is BoxCollider srcBox)
        {
            BoxCollider newCol = targetParent.gameObject.AddComponent<BoxCollider>();
            newCol.center = srcBox.center;
            newCol.size = srcBox.size;
            newCol.isTrigger = srcBox.isTrigger;
        }
        else if (sourceCollider is SphereCollider srcSphere)
        {
            SphereCollider newCol = targetParent.gameObject.AddComponent<SphereCollider>();
            newCol.center = srcSphere.center;
            newCol.radius = srcSphere.radius;
            newCol.isTrigger = srcSphere.isTrigger;
        }
        else if (sourceCollider is CapsuleCollider srcCapsule)
        {
            CapsuleCollider newCol = targetParent.gameObject.AddComponent<CapsuleCollider>();
            newCol.center = srcCapsule.center;
            newCol.radius = srcCapsule.radius;
            newCol.height = srcCapsule.height;
            newCol.direction = srcCapsule.direction;
            newCol.isTrigger = srcCapsule.isTrigger;
        }
        else if (sourceCollider is MeshCollider srcMesh)
        {
            MeshCollider newCol = targetParent.gameObject.AddComponent<MeshCollider>();
            newCol.sharedMesh = srcMesh.sharedMesh;
            newCol.convex = srcMesh.convex;
            newCol.isTrigger = srcMesh.isTrigger;
        }
        else
        {
            Debug.LogWarning($"Type de collider non pris en charge : {sourceCollider.GetType().Name}");
        }
    }
    
    #endregion

    private void CatchHidder(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Hidder"))
        {
            Debug.Log("Found!");
        }
    }
   

}

public struct PlayerData : INetworkSerializable
{
    public int life;
    public bool stunt;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref life);
        serializer.SerializeValue(ref stunt);
    }
}
