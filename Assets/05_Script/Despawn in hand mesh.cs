using System.Collections;
using UnityEngine;
using Meta.XR.Samples;
using OculusSampleFramework;

public class RemoveMeshButtonSafe : MonoBehaviour
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
            StartCoroutine(DestroyMeshDelayed(spawnScript.spawnedMesh));
        }
    }

    private IEnumerator DestroyMeshDelayed(GameObject mesh)
    {
        yield return null; // ©µ¿ð¤@´V¡AÁ×§K XR NullReference
        if (mesh != null)
            Destroy(mesh);

        spawnScript._ResetSpawn();
        Debug.Log("Hand mesh removed safely!");
    }
}
