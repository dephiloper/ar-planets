using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// @author Philipp Bönsch
/// calculates planet trajectories and applies velocities to planets when simulating
/// </summary>
public class PlanetManager : MonoBehaviour
{
    public float gravity = 0.001f;
    public static PlanetManager Instance { get; private set; }

    [SerializeField] private Button primaryActionButton;
    [SerializeField] private Button modeButton;
    [SerializeField] [Range(500, 10000)] private int range;
    [SerializeField] private GameObject collisionPrefab;

    private readonly bool _isMobile = Application.platform == RuntimePlatform.Android;
    private readonly List<Planet> _planets = new List<Planet>();
    private readonly List<GameObject> _collisionSpheres = new List<GameObject>();

    private int _index;
    private List<Vector3>[] _trajectory;
    private Vector3[] _velocities;
    private bool[] _collidedPlanets;
    private bool _forceUpdateTrajectory;
    private bool _isSimulating;
    private int _prevRange;
    private float _prevGravity;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        if (!_isMobile) return;

        primaryActionButton.onClick.AddListener(() =>
        {
            if (ModeManager.Instance.CurrentMode != ModeManager.Mode.Simulate) return;
            _isSimulating = !_isSimulating;
        });
    }

    private void Update() => CheckPropertiesChanged();

    private void FixedUpdate()
    {
        if (!_isSimulating)
        {
            if (UpdateTrajectoryNeeded())
                RedrawTrajectory();
            
            if (_isMobile)
                modeButton.interactable = _planets.All(p => p.IsSetup);
            else if (Input.GetKeyDown(KeyCode.Space))
                _isSimulating = !_isSimulating;
        }
        else
        {
            // loop over every planet and move it along its trajectory
            for (var pIndex = 0; pIndex < _planets.Count; pIndex++)
            {
                if (_planets[pIndex].HasCollided) continue;
                _collisionSpheres[pIndex].gameObject.SetActive(false);

                // set the new planet position
                _planets[pIndex].transform.position = _trajectory[pIndex][_index];
                UpdatePlanetTrajectory(pIndex);

                var collidedPlanets = _planets.Select(p => p.HasCollided).ToArray();
                var collidingPlanet = CheckForCollision(_index, pIndex, collidedPlanets);
                
                if (collidingPlanet != -1)
                {
                    _planets[pIndex].HasCollided = true;
                    _planets[collidingPlanet].HasCollided = true;
                }
            }

            _index++;
        }
    }
    
    /// <summary>
    /// gets called when a new planet is created
    /// </summary>
    /// <param name="planet">new registered planet</param>
    public void RegisterPlanet(Planet planet)
    {
        _planets.Add(planet);
        // adds a collision sphere for each planet
        _collisionSpheres.Add(Instantiate(collisionPrefab, Vector3.zero, Quaternion.identity));
    }
    
    /// <summary>
    /// checks if any property has changed its value and triggers an update of the trajectory
    /// </summary>
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

    /// <summary>
    /// checks if the trajectory has to be updated
    /// </summary>
    private bool UpdateTrajectoryNeeded()
    {
        var firstUpdate = _trajectory == null;
        var forceUpdate = _forceUpdateTrajectory;
        var planetsUpdated = _planets.Any(p => p.HasChanged);
        
        // reset everything
        _planets.ForEach(p => p.HasChanged = false);
        _forceUpdateTrajectory = false;
        return firstUpdate || planetsUpdated || forceUpdate;
    }

    /// <summary>
    /// redraws the trajectory of each planet
    /// </summary>
    private void RedrawTrajectory()
    {
        _trajectory = new List<Vector3>[_planets.Count];
        _velocities = new Vector3[_planets.Count];

        for (var i = 0; i < _planets.Count; i++)
        {
            _velocities[i] = _planets[i].initialVelocity;
            _trajectory[i] = new List<Vector3> {_planets[i].transform.position};
            _collisionSpheres[i].SetActive(false);
        }

        var collidedPlanets = new bool[_planets.Count];
        
        // for the specified range of the trajectory
        for (var i = 0; i < range; i++)
        {
            // loop over every planet
            for (var pIndex = 0; pIndex < _planets.Count; pIndex++)
            {
                // if planet has already been collided with another stop calculating further trajectory
                if (collidedPlanets[pIndex])
                    continue;
                
                UpdatePlanetTrajectory(pIndex);

                // check for collision between planets
                var collidingPlanet = CheckForCollision(i, pIndex, collidedPlanets);
                
                // if collision appeared show collision spheres
                if (collidingPlanet != -1)
                {
                    collidedPlanets[pIndex] = true;
                    _collisionSpheres[pIndex].SetActive(true);
                    _collisionSpheres[pIndex].transform.localScale = Vector3.one * (_planets[pIndex].radius * Planet.ScaleCoefficient);                    
                    _collisionSpheres[pIndex].transform.position = _trajectory[pIndex][i];

                    collidedPlanets[collidingPlanet] = true;
                    _collisionSpheres[collidingPlanet].SetActive(true);
                    _collisionSpheres[collidingPlanet].transform.localScale = Vector3.one * (_planets[collidingPlanet].radius * Planet.ScaleCoefficient);
                    _collisionSpheres[collidingPlanet].transform.position = _trajectory[collidingPlanet][i];
                }
            }
        }
        
        // update the lines based on the new calculated positions
        for (var i = 0; i < _planets.Count; i++)
        {
            var lineRenderer = _planets[i].GetComponent<LineRenderer>();
            lineRenderer.SetPositions(new Vector3[0]);
            lineRenderer.positionCount = _trajectory[i].Count;
            lineRenderer.SetPositions(_trajectory[i].ToArray());
        }
    }
    
    /// <summary>
    /// updates the velocity and upcoming positions of a planet
    /// </summary>
    /// <param name="pIndex">planet index</param>
    private void UpdatePlanetTrajectory(int pIndex)
    {
        // calculate forces depending on the current positions of the planets
        var latestPositions = _trajectory.Select(p => p.Last()).ToArray();
        var force = CalculateGravitationalForce(latestPositions, pIndex);
        var acceleration = force / _planets[pIndex].Mass;
        _velocities[pIndex] += acceleration;

        // update the new positions
        var newPosition = latestPositions[pIndex] + _velocities[pIndex] * Time.fixedDeltaTime;
        _trajectory[pIndex].Add(newPosition);
    }

    /// <summary>
    /// calculation of the gravitational force that emerges on one planet in relation the the other planets
    /// </summary>
    /// <param name="positions">current positions of all planets</param>
    /// <param name="pIndex">the considered planet index</param>
    /// <returns>the force that needs to be applied on the observed planet</returns>
    private Vector3 CalculateGravitationalForce(IReadOnlyList<Vector3> positions, int pIndex)
    {
        var pos = positions[pIndex];
        var force = Vector3.zero;
        var mass = _planets[pIndex].Mass;

        for (var i = 0; i < _planets.Count; i++)
        {
            if (pIndex == i || _planets[i].HasCollided) continue;
            var otherPos = positions[i];

            var distSqr = Vector3.SqrMagnitude(otherPos - pos);
            var dir = (otherPos - pos).normalized;
            
            // Newton's law of universal gravitation
            // F = G * (m1 * m2 / r * r)
            // F: force
            // G: Gravity
            // m1: mass of planet a
            // m2: mass of planet b
            // r: distance between these planets squared
            force += dir * (gravity * mass * _planets[i].Mass) / distSqr;
        }

        return force;
    }
    
    /// <summary>
    /// checks whether a collision occurs between one and any other planet
    /// <returns>the id of the other planet</returns>
    /// </summary>
    private int CheckForCollision(int currentIndex, int pIndex, IReadOnlyList<bool> collidedPlanets)
    {
        for (var i = 0; i < _planets.Count; i++)
        {
            // check collision only with other planets
            // and see if the other planet has not been collided with before
            if (i == pIndex || collidedPlanets[i]) continue;

            var pos = _trajectory[pIndex][currentIndex];
            var otherPos = _trajectory[i][currentIndex];

            if (Vector3.Distance(pos, otherPos) < _planets[pIndex].radius * 0.1 + _planets[i].radius * 0.1)
                return i;
        }

        return -1;
    }

    public void DeselectAllPlanets()
    {
        foreach (var p in _planets)
            p.IsSelected = false;
    }
}