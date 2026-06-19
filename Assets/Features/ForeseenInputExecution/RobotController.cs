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

        if(_isRotating) _transform.rotation = Quaternion.RotateTowards(_transform.rotation, _targetRot, _rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(_transform.rotation, _targetRot) > 0.1f) return;
        _transform.rotation = _targetRot;
        _isRotating = false;

    }

    #endregion


    #region Main API

    public void OnClick_MoveForwardCommand()
    {
        AddToInputQueue(new MoveForwardCommand());
        UpdateIndex(1);
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
        _currentRot = _transform.rotation;
        _targetRot = _currentRot * Quaternion.Euler(0f, angle, 0f);
        _isRotating = true;

    }


    private void AddToInputQueue(ICommandInputs command)
    {
        _playQueue.Enqueue(command);

    }

    public async Task PlayQueueAsync()
    {
        while (_playQueue.Count > 0)
        {
            ICommandInputs command = _playQueue.Dequeue();
            AddToUndoList(command);
            command.Execute(this);

            while (_isMoving || _isRotating)
            {
                await Task.Yield();
            }

            await Task.Delay((int)(_actionDuration * 1000));
        }

    }

    private void AddToUndoList(ICommandInputs command)
    {
        _undoStack.Push(command);
        
    }
    private void AddToRedoList(ICommandInputs command)
    {
        _redoStack.Push(command);
    }
    private void RemoveFromUndoList(ICommandInputs command)
    {
        _undoStack.Push(command);
    }
    private void RemoveFromRedoList(ICommandInputs command)
    {
        _redoStack.Push(command);
    }

    private void UpdateIndex(int info)
    {
        if(info == 0) return;
        if (info > 0)
        {
            for (int i = 0; i < info; i++)
            {
            _listIndex++;

            }
        }
        else
        {
            for (int i = _redoStack.Count - 1 ; i > info; i--)
            {
                _listIndex--;

            }
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

    private Quaternion _currentRot;
    private Quaternion _targetRot;
    private bool _isRotating;

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private float _actionDuration = 1f;

    private Queue<ICommandInputs> _playQueue = new();
    private Stack<ICommandInputs> _undoStack = new Stack<ICommandInputs>();
    private Stack<ICommandInputs> _redoStack = new Stack<ICommandInputs>();
    private int _listIndex;

    #endregion
}
