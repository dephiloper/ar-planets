using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlanetManager : MonoBehaviour
{
    public float gravity = 0.001f;
    public static PlanetManager Instance { get; private set; }

    [SerializeField] private bool isDebug;
    [SerializeField] private Button primaryActionButton;
    [SerializeField] [Range(50, 3000)] private int range;
    [SerializeField] private GameObject collisionPrefab;

    private readonly List<Planet> _planets = new List<Planet>();

    private int _prevRange;
    private float _prevGravity;
    private bool _forceUpdateTrajectory;
    private int _index;
    private List<Vector3>[] _points;
    private bool[] _collidedPlanets;
    private Vector3[] _velocities;
    private bool _isSimulating;
    private readonly List<GameObject> _collisionSpheres = new List<GameObject>();

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

        primaryActionButton.onClick.AddListener(() =>
        {
            if (ModeManager.Instance.CurrentMode != ModeManager.Mode.Simulate) return;
            
            _isSimulating = !_isSimulating;
        });
    }

    public void RegisterPlanet(Planet planet)
    {
        _planets.Add(planet);
        var sphere = Instantiate(collisionPrefab, Vector3.zero, Quaternion.identity);
        _collisionSpheres.Add(sphere);
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
        
        if (Math.Abs(_prevGravity - gravity) > 0.000001f)
        {
            _forceUpdateTrajectory = true;
            _prevGravity = gravity;
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
                primaryActionButton.interactable = _planets.All(p => p.IsSetup);
            }
        }
        else
        {
            for (var pIndex = 0; pIndex < _planets.Count; pIndex++)
            {
                if (_planets[pIndex].HasCollided) continue;
                
                _collisionSpheres[pIndex].gameObject.SetActive(false);
                
                _planets[pIndex].transform.position = _points[pIndex][_index];
                
                var latestPositions = FindCurrentPlanetPositions();
                var force = CalculateGravitationalForce(latestPositions, pIndex);

                _velocities[pIndex] += force;
                _points[pIndex].Add(_points[pIndex].Last() + _velocities[pIndex] * Time.fixedDeltaTime);

                var x = _planets.Select(p => p.HasCollided).ToArray();
                var collidingPlanet = CheckForCollision(_index, pIndex, x);
                
                if (collidingPlanet != -1)
                {
                    _planets[pIndex].HasCollided = true;
                    _planets[collidingPlanet].HasCollided = true;
                }
            }

            _index++;
        }
    }

    private bool UpdateTrajectoryNeeded()
    {
        var firstUpdate = _points == null;
        var forceUpdate = _forceUpdateTrajectory;
        var planetsUpdated = _planets.Any(p => p.HasChanged);
        
        // reset everything
        _planets.ForEach(p => p.HasChanged = false);
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
            _collisionSpheres[i].SetActive(false);
        }

        var collidedPlanets = new bool[_planets.Count];
        
        for (var i = 0; i < range; i++)
        {
            for (var pIndex = 0; pIndex < _planets.Count; pIndex++)
            {
                // if planet has already been collided with another stop calculating further trajectory
                if (collidedPlanets[pIndex])
                    continue;
                
                var latestPositions = FindCurrentPlanetPositions();
                var force = CalculateGravitationalForce(latestPositions, pIndex);

                _velocities[pIndex] += force;
                var newPosition = _points[pIndex].Last() + _velocities[pIndex] * Time.fixedDeltaTime;
                _points[pIndex].Add(newPosition);

                var collidingPlanet = CheckForCollision(i, pIndex, collidedPlanets);
                
                // if collision appeared 
                if (collidingPlanet != -1)
                {
                    collidedPlanets[pIndex] = true;
                    _collisionSpheres[pIndex].SetActive(true);
                    _collisionSpheres[pIndex].transform.localScale = Vector3.one * (_planets[pIndex].radius * 0.2f);                    
                    _collisionSpheres[pIndex].transform.position = _points[pIndex][i];

                    collidedPlanets[collidingPlanet] = true;
                    _collisionSpheres[collidingPlanet].SetActive(true);
                    _collisionSpheres[collidingPlanet].transform.localScale = Vector3.one * (_planets[collidingPlanet].radius * 0.2f);
                    _collisionSpheres[collidingPlanet].transform.position = _points[collidingPlanet][i];
                }
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

    private int CheckForCollision(int currentIndex, int pIndex, IReadOnlyList<bool> collidedPlanets)
    {
        for (var i = 0; i < _planets.Count; i++)
        {
            // check collision only with other planets
            // and see if the other planet has not been collided with before
            if (i == pIndex || collidedPlanets[i]) continue;

            var pos = _points[pIndex][currentIndex];
            var otherPos = _points[i][currentIndex];

            if (Vector3.Distance(pos, otherPos) < _planets[pIndex].radius * 0.1 + _planets[i].radius * 0.1)
                return i;
        }

        return -1;
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
        var mass = _planets[pIndex].Mass;

        for (var i = 0; i < _planets.Count; i++)
        {
            if (pIndex == i || _planets[i].HasCollided) continue;
            var otherPos = positions[i];
            var distSqr = Vector3.SqrMagnitude(pos - otherPos);
            var dir = (otherPos - pos).normalized;
            force += dir * (gravity * (mass * _planets[i].Mass / distSqr));
        }

        return force;
    }

    public void DeselectAllPlanets()
    {
        foreach (var p in _planets)
            p.IsSelected = false;
    }
}