using System.Collections;
using UnityEngine;
using Meta.XR.Samples;
using OculusSampleFramework;

public class FreezeMeshButtonSafe : MonoBehaviour
{
    [SerializeField] private ButtonController targetButton;
    [SerializeField] private SpawnOnHandSafe spawnScript;

    private void OnEnable()
    {
        if (targetButton != null)
            targetButton.InteractableStateChanged.AddListener(OnButtonStateChanged);
    }

    private void OnDisable()
    {
        if (targetButton != null)
            targetButton.InteractableStateChanged.RemoveListener(OnButtonStateChanged);
    }

    private void OnButtonStateChanged(InteractableStateArgs args)
    {
        if (args.NewInteractableState == InteractableState.ActionState && spawnScript != null && spawnScript.spawnedMesh != null)
        {
            StartCoroutine(FreezeMeshDelayed(spawnScript.spawnedMesh));
        }
    }

    private IEnumerator FreezeMeshDelayed(GameObject mesh)
    {
        yield return null; // 延遲一幀，避免 XR NullReference

        if (mesh != null)
        {
            mesh.transform.SetParent(null); // 從手上解除父物件
            Rigidbody rb = mesh.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true; // 固定在世界座標
        }

        spawnScript._ResetSpawn(); // 解除生成狀態，手上沒有持有物件
        Debug.Log("Hand mesh frozen safely!");
    }
}
