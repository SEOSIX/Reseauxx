using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PropMorpher : NetworkBehaviour
{
    [SerializeField] private Mesh[] availableMeshes;
    [SerializeField] private int[] lifeValues;
    [SerializeField] private Material[] avaiablesMaterials;
    private LifeManager lifeManager;
    private GameObject lastHitObject;
    private Renderer playerRenderer;
    private Material currentMaterial;


    private List<GameObject> copies = new List<GameObject>();
    
    private List<int> meshDuplicated = new List<int>();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        if (lifeManager == null)
            lifeManager = LifeManager.instance;
        var meshFilter = GetComponentInChildren<MeshFilter>();
        if (meshFilter != null && availableMeshes != null && lifeValues != null)
        {
            int meshIndex = System.Array.IndexOf(availableMeshes, meshFilter.sharedMesh);
            if (meshIndex >= 0 && meshIndex < lifeValues.Length)
            {
                lifeManager.SetLife(lifeValues[meshIndex]);
            }
            else
            {
                lifeManager.SetLife(lifeValues[0]);
            }
        }
    }
    private void Start()
    {
        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
            currentMaterial = playerRenderer.sharedMaterial;

        lifeManager = LifeManager.instance;
    }

    private void Update()
    {
        DefRayCast();
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            RequestDuplicateServerRpc();
        }
    }

    private bool isAimingObject = false;
    private bool hasReplacedBeforeCenter = false;
    private GameObject currentTarget;

    void DefRayCast()
    {
        if (!IsOwner) return;

        Vector3 camForward = Camera.main.transform.forward;

        if (Physics.Raycast(Camera.main.transform.position, camForward, out RaycastHit hit, 10))
        {
            if (hit.collider.CompareTag("ObjectToTransform"))
            {
                GameObject target = hit.collider.gameObject;
                if (currentTarget != target)
                {
                    currentTarget = target;
                    isAimingObject = true;
                    hasReplacedBeforeCenter = false;
                    Cursor.instance.RecenterCursor();
                }

                HighlightObject(target);
                if (Input.GetMouseButton(0))
                {
                    NetworkObject targetNetObj = hit.collider.GetComponent<NetworkObject>();
                    if (targetNetObj != null)
                        RequestMorphServerRpc(targetNetObj.NetworkObjectId);
                }
            }
            else
            {
                ResetCursorAndHighlight();
            }
        }
        else
        {
            ResetCursorAndHighlight();
        }
    }

    private void ResetCursorAndHighlight()
    {
        if (isAimingObject)
        {
            isAimingObject = false;
            hasReplacedBeforeCenter = false;
            currentTarget = null;
            Cursor.instance.ReplaceCursor();
        }
        ClearHighlight();
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
            
            var targetRenderer = target.GetComponentInChildren<Renderer>();
            var targetMaterial = targetRenderer != null ? targetRenderer.material : null;

            ApplyMorph(meshIndex, col, scale);
            ApplyMorphClientRpc(meshIndex, scale, targetMaterial.mainTexture.name);
        }
    }

    [ServerRpc]
    private void RequestDuplicateServerRpc(ServerRpcParams rpcParams = default)
    {
        var mf = GetComponentInChildren<MeshFilter>();
        if (mf == null || availableMeshes == null || availableMeshes.Length == 0) return;

        var currentMesh = mf.sharedMesh;
        int meshIndex = System.Array.IndexOf(availableMeshes, currentMesh);
        if (meshIndex < 0) meshIndex = 0;

        DuplicateMeshClientRpc(meshIndex, transform.position, transform.rotation);
    }

    [ClientRpc]
    private void ApplyMorphClientRpc(int meshIndex, Vector3 scale, string materialName)
    {
        ApplyMesh(meshIndex);
        transform.localScale = scale;

        if (!string.IsNullOrEmpty(materialName))
        {
            Material mat = Resources.Load<Material>(materialName);
            if (mat != null)
            {
                var r = GetComponentInChildren<Renderer>();
                if (r != null)
                    r.material = mat;
            }
        }

        ApplyLife(meshIndex);
    }
    
    [ClientRpc]
    private void DuplicateMeshClientRpc(int meshIndex, Vector3 pos, Quaternion rot)
    {
        GameObject copy = new GameObject("MeshCopy");

        var mf = copy.AddComponent<MeshFilter>();
        mf.mesh = availableMeshes[meshIndex];

        var mr = copy.AddComponent<MeshRenderer>();
        mr.material = avaiablesMaterials[meshIndex];

        copy.transform.SetPositionAndRotation(pos, rot);
        copy.isStatic = true;

        copies.Add(copy);
        meshDuplicated.Add(1);

        StartCoroutine(DestroyCopyAfterDelay(copy));
    }
    
    private void ApplyMorph(int meshIndex, Collider col, Vector3 scale)
    {
        ApplyMesh(meshIndex);
        CopyCollider(col);
        transform.localScale = scale;
        ApplyLife(meshIndex);
    }

    private void ApplyMesh(int index)
    {
        if (index >= 0 && index < availableMeshes.Length)
        {
            GetComponentInChildren<MeshFilter>().mesh = availableMeshes[index];
        }
        ApplyMaterial(index);
    }

    public void ApplyLife(int index)
    {
    if (lifeManager == null) return;
    
    int newLife = lifeValues[index];
        lifeManager.playerSlider.maxValue = lifeValues[index]; 
        lifeManager.SetLife(lifeValues[index]);        
        lifeManager.SetMaxLife(lifeValues[index]);
        if (IsOwner)
        {
            GetComponent<PlayerNetwork>().SetLifeServerRpc(newLife);
        }
    }


    private void ApplyMaterial(int index)
    {
        if (index < 0 || index >= avaiablesMaterials.Length) return;

        var r = GetComponentInChildren<Renderer>();
        Material material = r.material;
        if (r != null)
        {
            r.material.mainTexture = avaiablesMaterials[index].mainTexture;
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

    private IEnumerator DestroyCopyAfterDelay(GameObject copy)
    {
        yield return new WaitForSeconds(15);

        if (copies.Contains(copy))
            copies.Remove(copy);

        Destroy(copy);
        if (meshDuplicated.Count > 0)
            meshDuplicated.RemoveAt(0);
    }

    private void CopyCollider(Collider source)
    {
        Transform meshTransform = GetComponentInChildren<MeshFilter>().transform;
        
        foreach (var old in meshTransform.GetComponents<Collider>())
            Destroy(old);

        if (source is BoxCollider box)
        {
            var newCol = meshTransform.gameObject.AddComponent<BoxCollider>();
            newCol.center = box.center;
            newCol.size = box.size;
        }
        else if (source is SphereCollider sphere)
        {
            var newCol = meshTransform.gameObject.AddComponent<SphereCollider>();
            newCol.center = sphere.center;
            newCol.radius = sphere.radius;
        }
        else if (source is CapsuleCollider capsule)
        {
            var newCol = meshTransform.gameObject.AddComponent<CapsuleCollider>();
            newCol.center = capsule.center;
            newCol.radius = capsule.radius;
            newCol.height = capsule.height;
            newCol.direction = capsule.direction;
        }
        else if (source is MeshCollider meshCollider)
        {
            var newCol = meshTransform.gameObject.AddComponent<MeshCollider>();
            newCol.convex = true;
            newCol.sharedMesh = meshCollider.sharedMesh;
        }
    }
}
