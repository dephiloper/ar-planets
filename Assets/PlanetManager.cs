using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlanetManager : MonoBehaviour
{
    private const float Gravity = 0.001f;
   
    [SerializeField] private bool isDebug;
    [SerializeField] private Button spawnButton;
    [SerializeField] private Button startButton;
    [SerializeField] [Range(50, 3000)] private int range;

    public static PlanetManager Instance { get; private set; }
    private readonly List<Planet> _planets = new List<Planet>();

    private int _prevRange;
    private bool _forceUpdateTrajectory;
    private int _index;
    private List<Vector3>[] _points;
    private Vector3[] _velocities;
    private bool _isSimulating;
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (isDebug) return;

        startButton.onClick.AddListener(() =>
        {
            _isSimulating = !_isSimulating;
            spawnButton.interactable = _isSimulating;
            startButton.GetComponentInChildren<Text>().text = _isSimulating ? "Start" : "Stop";
        });
    }

    public void RegisterPlanet(Planet planet)
    {
        _planets.Add(planet);
    }

    private void Update()
    {
        CheckPropertiesChanged();
    }

    private void CheckPropertiesChanged()
    {
        if (_prevRange != range)
        {
            _forceUpdateTrajectory = true;
            _prevRange = range;
        }
    }

    private void FixedUpdate()
    {
        if (!_isSimulating)
        {
            if (UpdateTrajectoryNeeded())
            {
                RedrawTrajectory();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isSimulating = true;
            }

            if (!isDebug)
            {
                startButton.interactable = _planets.All(p => p.isSetup);
            }
        }
        else
        {
            for (var pIndex = 0; pIndex < _planets.Count; pIndex++)
            {
                _planets[pIndex].transform.position = _points[pIndex][_index];

                var latestPositions = FindCurrentPlanetPositions();
                var force = CalculateGravitationalForce(latestPositions, pIndex);

                _velocities[pIndex] += force;
                _points[pIndex].Add(_points[pIndex].Last() + _velocities[pIndex] * Time.fixedDeltaTime);
            }

            _index++;
        }
    }

    private bool UpdateTrajectoryNeeded()
    {
        var firstUpdate = _points == null;
        var forceUpdate = _forceUpdateTrajectory;
        var planetsUpdated = _planets.Any(p => p.hasChanged);
        
        // reset everything
        _planets.ForEach(p => p.hasChanged = false);
        _forceUpdateTrajectory = false;
        return firstUpdate || planetsUpdated || forceUpdate;
    }

    private void RedrawTrajectory()
    {
        _points = new List<Vector3>[_planets.Count];
        _velocities = new Vector3[_planets.Count];

        for (var i = 0; i < _planets.Count; i++)
        {
            _velocities[i] = _planets[i].initialVelocity;
            _points[i] = new List<Vector3> {_planets[i].transform.position};
        }

        for (var i = 0; i < range; i++)
        {
            for (var pIndex = 0; pIndex < _planets.Count; pIndex++)
            {
                var latestPositions = FindCurrentPlanetPositions();
                var force = CalculateGravitationalForce(latestPositions, pIndex);

                _velocities[pIndex] += force;
                _points[pIndex].Add(_points[pIndex].Last() + _velocities[pIndex] * Time.fixedDeltaTime);
            }
        }

        for (var i = 0; i < _planets.Count; i++)
        {
            var lineRenderer = _planets[i].GetComponent<LineRenderer>();
            lineRenderer.SetPositions(new Vector3[0]);
            lineRenderer.positionCount = _points[i].Count;
            lineRenderer.SetPositions(_points[i].ToArray());
        }
    }

    private Vector3[] FindCurrentPlanetPositions()
    {
        var currentPositions = new Vector3[_planets.Count];
        for (var i = 0; i < _planets.Count; i++)
            currentPositions[i] = _points[i].Last();

        return currentPositions;
    }

    private Vector3 CalculateGravitationalForce(IReadOnlyList<Vector3> positions, int pIndex)
    {
        var pos = positions[pIndex];
        var force = Vector3.zero;
        var mass = _planets[pIndex].mass;

        for (var i = 0; i < _planets.Count; i++)
        {
            if (pIndex == i) continue;
            var otherPos = positions[i];
            var distSqr = Vector3.SqrMagnitude(pos - otherPos);
            var dir = (otherPos - pos).normalized;
            force += dir * (Gravity * (mass * _planets[i].mass / distSqr));
        }

        return force;
    }
}