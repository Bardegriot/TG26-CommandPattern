using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RobotController : MonoBehaviour
{
    #region Publics

    #endregion


    #region Unity API

    private void Start()
    {
        _transform = GetComponent<Transform>();
    }


    private void Update()
    {
        _currentPos = _transform.position;
        

        if (_isMoving) _transform.position = Vector3.MoveTowards(_currentPos, _targetPos, _moveSpeed * Time.deltaTime);
        //if (_isRotating) _transform.Rotate = Vector3.MoveTowards(_currentPos, _targetPos, _moveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(_transform.position, _targetPos) >= 0.001f) return;
        _isMoving = false;
    }

    #endregion


    #region Main API

    public void OnClick_MoveForwardCommand()
    {
        AddToInputQueue(new MoveForwardCommand());
    }

    public void OnClick_RotateCommand(float angle)
    {
        AddToInputQueue(new RotateCommand(angle));
    }

    public void OnClick_PlaySequence()
    {
        PlayQueueAsync();
    }

    public void MoveForward()
    {
        _targetPos = _currentPos + _transform.forward;
        _targetPos.y = _currentPos.y;
        _isMoving = true;
    }

    public void Rotate(float angle)
    {
        _transform.Rotate(0f, angle, 0f);

    }


    private void AddToInputQueue(ICommandInputs command)
    {
        //if (_playQueue.Count > 0) _playQueue.Clear();
        _playQueue.Enqueue(command);
        //command.AddedToInputQueue();
    }

    public async Task PlayQueueAsync()
    {
        while (_playQueue.Count > 0)
        {
            ICommandInputs command = _playQueue.Dequeue();
            command.Execute(this);
            await Task.Delay((int)(_actionDuration * 1000));
        }

    }

    public void Execute(ICommandInputs command)
    {

    }


    #endregion


    #region Private and Protected

    private Transform _transform;
    private Vector3 _rotationAngle; 

    private Vector3 _currentPos;
    private Vector3 _targetPos;

    private bool _isMoving;
    private bool _isRotating;

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _actionDuration = 1f;

    private Queue<ICommandInputs> _playQueue = new();

    #endregion
}
