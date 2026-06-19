using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ajc.review.playerMovement
{
    public class CommandBasedPlayerMovement : MonoBehaviour, IMoveCommandReceiver, IRotationCommandReceiver
    {
        private List<IPlayerCommand> _commands = new();
        [SerializeField] private float _speed =1;
        [SerializeField] private float _angularVelocity =50;

        private IEnumerator Start()
        {
            _commands.Add(new MoveForwardCommand(this,2));
            _commands.Add(new RotateCommand(this,90));
            _commands.Add(new MoveForwardCommand(this,2));
            _commands.Add(new MoveBackwardCommand(this,2));
            _commands.Add(new RotateCommand(this,-90));
            
            yield return new WaitForSeconds(1);
            
            foreach (var command in _commands)
            {
                yield return command.Execute();
                yield return new WaitForSeconds(1);
            }
            Debug.Log("CommandBasedPlayerMovement - FINITO");
        }
        
        void Update()
        {
        
        }

        public IEnumerator MoveCoroutine(Vector3 direction, float distance)
        {
            var initialPosition = transform.position;
            var localDirection = transform.TransformDirection(direction);
            
            var progression = 0f;
            while (progression<distance)
            {
                progression += Time.deltaTime*_speed;
                transform.position = initialPosition + localDirection*progression;
                yield return null;
            }
            transform.position = initialPosition+ distance*localDirection;
        }

        public IEnumerator RotateCoroutine(float angle)
        {
            var rotationDirection = angle > 0 ? 1 : -1;
            var initialRotation = transform.rotation.eulerAngles;
            var progression = 0f;
            while (progression<Mathf.Abs(angle))
            {
                progression += Time.deltaTime*_angularVelocity;
                transform.rotation = Quaternion.Euler(
                    initialRotation.x,
                    initialRotation.y+rotationDirection*progression,
                    initialRotation.z);
                yield return null;
            }
            transform.rotation = Quaternion.Euler(
                initialRotation.x,
                initialRotation.y+angle,
                initialRotation.z);
        }
    }

    public interface IRotationCommandReceiver
    {
        IEnumerator RotateCoroutine(float angle);
    }
    public interface IMoveCommandReceiver
    {
        IEnumerator MoveCoroutine(Vector3 direction, float distance);
    }

    public interface IPlayerCommand
    {
        IEnumerator Execute();
    }

    public class MoveForwardCommand : IPlayerCommand
    {
        private readonly IMoveCommandReceiver _target;
        private readonly float _distance;

        public MoveForwardCommand(IMoveCommandReceiver receiver,  float distance)
        {
            _target = receiver;
            _distance = distance;
        }

        public IEnumerator Execute()
        {
            yield return _target.MoveCoroutine(Vector3.forward,_distance);
        }
    }
    
    public class RotateCommand : IPlayerCommand
    {
        private readonly IRotationCommandReceiver _target;
        private readonly float _angle;

        public RotateCommand(IRotationCommandReceiver receiver, float angle)
        {
            _target = receiver;
            _angle = angle;
        }
        public IEnumerator Execute()
        {
            yield return _target.RotateCoroutine(_angle);
        }
    }
    
    public class MoveBackwardCommand : IPlayerCommand
    {
        private readonly IMoveCommandReceiver _receiver;
        private readonly float _distance;

        public MoveBackwardCommand(IMoveCommandReceiver receiver, float distance)
        {
            _receiver = receiver;
            _distance = distance;
        }
        public IEnumerator Execute()
        {
            yield return _receiver.MoveCoroutine(Vector3.back,_distance);
        }
    }
}

