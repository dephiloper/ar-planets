using System;
using UnityEngine;
using UnityEngine.UI;

public class GrabController : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private Camera arCamera;
    [SerializeField] private Button primaryActionButton;
    private GameObject _selectedPlanet;
    private bool _grabbing;

    private void Start()
    {
        primaryActionButton.onClick.AddListener(() =>
        {
            if (ModeManager.Instance.CurrentMode == ModeManager.Mode.Edit && _selectedPlanet != null)
            {
                _grabbing = !_grabbing;
                _selectedPlanet.transform.parent = _grabbing ? arCamera.transform : null;
                ModeManager.Instance.ChangeGrabImage(_grabbing);
            }
        });
    }

    private void Update()
    {
        PlanetManager.Instance.DeselectAllPlanets();

        if (ModeManager.Instance.CurrentMode != ModeManager.Mode.Edit) return;

        var ray = arCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out var hit))
        {
            var planet = hit.transform.GetComponent<Planet>();
            planet.IsSelected = true;
            _selectedPlanet = planet.gameObject;
        }
        else
        {
            _selectedPlanet = null;
        }
    }
}