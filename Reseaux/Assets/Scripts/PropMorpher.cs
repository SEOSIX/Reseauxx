using Unity.Netcode;
using UnityEngine;

public class PropMorpher : NetworkBehaviour
{
    [SerializeField] private Mesh[] availableMeshes;
    private GameObject lastHitObject;
    private Renderer playerRenderer;

    private void Start()
    {
        playerRenderer = GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
       DefRayCast();
    }

void DefRayCast(){
 if (!IsOwner) return;

    Vector3 camForward = Camera.main.transform.forward;
    if (Physics.Raycast(Camera.main.transform.position, camForward, out RaycastHit hit, 10))
    {
        if (hit.collider.CompareTag("ObjectToTransform"))
        {
            HighlightObject(hit.collider.gameObject);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                NetworkObject targetNetObj = hit.collider.GetComponent<NetworkObject>();
                if (targetNetObj != null)
                    RequestMorphServerRpc(targetNetObj.NetworkObjectId);
            }
        }
        else
        {
            ClearHighlight();
        }
    }
    else
    {
        ClearHighlight();
    }
}

    [ServerRpc]
    private void RequestMorphServerRpc(ulong targetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject target))
        {
            var mesh = target.GetComponentInChildren<MeshFilter>().sharedMesh;
            int meshIndex = System.Array.IndexOf(availableMeshes, mesh);
            var col = target.GetComponentInChildren<Collider>();
            Vector3 scale = target.transform.localScale;

            ApplyMorph(meshIndex, col, scale);
            ApplyMorphClientRpc(meshIndex, scale);
        }
    }

    [ClientRpc]
    private void ApplyMorphClientRpc(int meshIndex, Vector3 scale)
    {
        if (IsOwner) return;
        ApplyMesh(meshIndex);
        transform.localScale = scale;
    }

    private void ApplyMorph(int meshIndex, Collider col, Vector3 scale)
    {
        ApplyMesh(meshIndex);
        CopyCollider(col);
        transform.localScale = scale;
    }

    private void ApplyMesh(int index)
    {
        if (index >= 0 && index < availableMeshes.Length)
        {
            GetComponentInChildren<MeshFilter>().mesh = availableMeshes[index];
        }
    }

    private void HighlightObject(GameObject go)
    {
        if (lastHitObject == go) return;
        if (lastHitObject) lastHitObject.GetComponent<Renderer>().material.color = Color.white;
        go.GetComponent<Renderer>().material.color = Color.cyan;
        lastHitObject = go;
    }

    private void ClearHighlight()
    {
        if (lastHitObject)
        {
            lastHitObject.GetComponent<Renderer>().material.color = Color.white;
            lastHitObject = null;
        }
    }

    private void CopyCollider(Collider source)
    {
        foreach (var old in GetComponentsInChildren<Collider>())
            Destroy(old);

        if (source is BoxCollider box)
        {
            var newCol = gameObject.AddComponent<BoxCollider>();
            newCol.center = box.center;
            newCol.size = box.size;
        }
        else if (source is SphereCollider sphere)
        {
            var newCol = gameObject.AddComponent<SphereCollider>();
            newCol.center = sphere.center;
            newCol.radius = sphere.radius;
        }
        else if (source is CapsuleCollider capsule)
        {
            var newCol = gameObject.AddComponent<CapsuleCollider>();
            newCol.center = capsule.center;
            newCol.radius = capsule.radius;
        }
        else if (source is MeshCollider meshCollider)
        {
            var newCol = gameObject.AddComponent<MeshCollider>();
            newCol.convex = true;
            newCol.sharedMesh = meshCollider.sharedMesh;
        }
    }
}
