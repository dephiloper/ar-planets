using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARRaycastManager))]
public class PlacementController : MonoBehaviour
{
    private ARRaycastManager _arRaycastManager;

    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject placeIndicator;
    [SerializeField] private Button spawnButton;
    
    private void Awake()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
    }

    private void Start()
    {
        spawnButton.onClick.AddListener(() =>
        {
            if (ModeManager.Instance.CurrentMode == ModeManager.Mode.Place)
                Instantiate(planetPrefab, placeIndicator.transform.position, placeIndicator.transform.rotation);
        });
    }

    private void Update()
    {
        var showIndicator = false;
        if (Application.isMobilePlatform && ModeManager.Instance.CurrentMode == ModeManager.Mode.Place)
        {

            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hits = new List<ARRaycastHit>();

            if (_arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
            {
                if (hits.Count > 0)
                {
                    var hitPose = hits[0].pose;
                    showIndicator = true;
                    placeIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                }
            }
        }

        placeIndicator.SetActive(showIndicator);
    }
}