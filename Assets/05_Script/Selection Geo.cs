using Meta.XR.Samples;
using UnityEngine;

namespace OculusSampleFramework
{
    [MetaCodeSample("StarterSample-HandsTrain")]
    public class SelectionGeo : MonoBehaviour
    {
        public enum SelectionState
        {
            Off = 0,
            Selected,
            Highlighted
        }

        [SerializeField] private MeshRenderer _selectionMeshRenderer = null;

        private static int _colorId = Shader.PropertyToID("_Color");
        private Material[] _selectionMaterials;
        private Color[] _defaultSelectionColors = null, _highlightColors = null;

        private SelectionState _currSelectionState = SelectionState.Off;

        public SelectionState CurrSelectionState
        {
            get => _currSelectionState;
            set
            {
                if (_currSelectionState == value) return;

                _currSelectionState = value;
                if (_currSelectionState > SelectionState.Off)
                {
                    if (_selectionMeshRenderer != null)
                    {
                        _selectionMeshRenderer.enabled = true;
                        AffectSelectionColor(
                            _currSelectionState == SelectionState.Selected
                                ? _defaultSelectionColors
                                : _highlightColors
                        );
                    }
                }
                else
                {
                    if (_selectionMeshRenderer != null)
                        _selectionMeshRenderer.enabled = false;
                }
            }
        }

        private void Awake()
        {
            if (_selectionMeshRenderer == null)
            {
                Debug.LogWarning("SelectionCylinder: Missing MeshRenderer!");
                return;
            }

            // ✅ 建立獨立材質副本，避免共享引用
            var originalMats = _selectionMeshRenderer.sharedMaterials;
            _selectionMaterials = new Material[originalMats.Length];
            for (int i = 0; i < originalMats.Length; i++)
            {
                _selectionMaterials[i] = new Material(originalMats[i]);
            }
            _selectionMeshRenderer.materials = _selectionMaterials;

            int numColors = _selectionMaterials.Length;
            _defaultSelectionColors = new Color[numColors];
            _highlightColors = new Color[numColors];

            for (int i = 0; i < numColors; i++)
            {
                _defaultSelectionColors[i] = _selectionMaterials[i].GetColor(_colorId);
                _highlightColors[i] = new Color(1f, 1f, 1f, _defaultSelectionColors[i].a);
            }

            CurrSelectionState = SelectionState.Off;
        }

        private void OnDestroy()
        {
            // ✅ 不強制 Destroy 材質，避免 GPU pipeline crash
        }

        private void AffectSelectionColor(Color[] newColors)
        {
            for (int i = 0; i < _selectionMaterials.Length; i++)
            {
                if (_selectionMaterials[i] != null)
                    _selectionMaterials[i].SetColor(_colorId, newColors[i]);
            }
        }
    }
}
