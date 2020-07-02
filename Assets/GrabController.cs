using UnityEngine;
using UnityEngine.UI;

public class GrabController : MonoBehaviour
{
    [SerializeField] private Button primaryActionButton;
    [SerializeField] private Button secondaryActionButton;
    [SerializeField] private GameObject editPanel;
    private GameObject _selectedPlanet;
    private bool _grabbing;

    private void Start()
    {
        primaryActionButton.onClick.AddListener(() =>
        {
            if (ModeManager.Instance.CurrentMode == ModeManager.Mode.Edit && (_selectedPlanet != null || _grabbing))
            {
                _grabbing = !_grabbing;
                _selectedPlanet.transform.parent = _grabbing ? Camera.current.transform : null;
                ModeManager.Instance.ChangeGrabImage(_grabbing);
            }
        });
        
        secondaryActionButton.onClick.AddListener(() =>
        {
            if (ModeManager.Instance.CurrentMode == ModeManager.Mode.Edit && _selectedPlanet != null)
            {
                editPanel.SetActive(true);
                ModeManager.Instance.ShowButtons(false);
                editPanel.GetComponent<EditPlanetController>().SelectedPlanet = _selectedPlanet.GetComponent<Planet>();
            }
        });
    }

    private void Update()
    {
        PlanetManager.Instance.DeselectAllPlanets();

        if (ModeManager.Instance.CurrentMode != ModeManager.Mode.Edit) return;

        var ray = Camera.current.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out var hit))
        {
            var planet = hit.transform.parent.GetComponent<Planet>();
            if (planet != null)
            {
                planet.IsSelected = true;
                _selectedPlanet = planet.gameObject;
            }
        }
        else
        {
            _selectedPlanet = null;
        }

        primaryActionButton.interactable = _selectedPlanet != null || _grabbing;
        secondaryActionButton.interactable = _selectedPlanet != null || _grabbing;
    }
}