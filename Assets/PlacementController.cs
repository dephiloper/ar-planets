using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// @author Philipp Bönsch
/// controls initial placement of planets on the floor
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class PlacementController : MonoBehaviour
{
    private ARRaycastManager _arRaycastManager;

    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject placeIndicator;
    [SerializeField] private Button spawnButton;
    private bool _showIndicator;

    private void Start()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
        
        spawnButton.onClick.AddListener(() =>
        {
            // spawn planet when indicator visible
            if (ModeManager.Instance.CurrentMode == ModeManager.Mode.Place && _showIndicator)
                Instantiate(planetPrefab, placeIndicator.transform.position, placeIndicator.transform.rotation);
        });
    }

    private void Update()
    {
        _showIndicator = false;
        if (Application.isMobilePlatform && ModeManager.Instance.CurrentMode == ModeManager.Mode.Place)
        {
            // shoot a ar-raycast from the center of the screen and collide with ar planes only
            // when a plane was hit show the indicator
            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hits = new List<ARRaycastHit>();

            if (_arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
            {
                if (hits.Count > 0)
                {
                    var hitPose = hits[0].pose;
                    _showIndicator = true;
                    placeIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                }
            }
        }

        placeIndicator.SetActive(_showIndicator);
    }
}