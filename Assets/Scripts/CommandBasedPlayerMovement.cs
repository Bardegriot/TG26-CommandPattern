using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ajc.review.playerMovement
{
    public class CommandBasedPlayerMovement : MonoBehaviour, IMoveCommandReceiver, IRotationCommandReceiver
    {
        private List<IPlayerCommand> _commands = new();
        [SerializeField, Range(1,5)] private float _speed =1;
        [SerializeField, Range(50,10)] private float _angularVelocity =50;
        [SerializeField, Range(0,3)] private float _timeMultiplier = 1;
        private bool _endGameAtNextStep;

        private IEnumerator Start()
        {
            
            _commands.Add(new MoveForwardCommand(this,2));
            _commands.Add(new RotateCommand(this,90));
            _commands.Add(new MoveForwardCommand(this,2));
            
            _commands.Add(new RotateCommand(this,-90));
            _commands.Add(new MoveForwardCommand(this,2));
            
            foreach (var command in _commands)
            {
                yield return command.Execute();
                if(_endGameAtNextStep) yield break;
                yield return new WaitForSeconds(1/_timeMultiplier);
            }

            /*yield return new WaitForSeconds(1/_timeMultiplier);
            
            for (var i = _commands.Count - 1; i >= 0; i--)
            {
                yield return _commands[i].Revert();
                yield return new WaitForSeconds(1/_timeMultiplier);
            }*/
            
        }
        
        void Update()
        {
        
        }

        public IEnumerator MoveCoroutine(Vector3 direction, float distance)
        {
            var initialPosition = transform.position;
            var localDirection = transform.TransformDirection(direction);
            var destination = initialPosition + localDirection*distance;
            if (!ThisDestinationIsValid(destination)) _endGameAtNextStep = true;
            var progression = 0f;
            while (progression<distance)
            {
                progression += Time.deltaTime*_speed*_timeMultiplier;
                transform.position = initialPosition + localDirection*progression;
                yield return null;
            }
            transform.position = initialPosition+ distance*localDirection;
        }

        private bool ThisDestinationIsValid(Vector3 destination)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator RotateCoroutine(float angle)
        {
            var rotationDirection = angle > 0 ? 1 : -1;
            var initialRotation = transform.rotation.eulerAngles;
            var progression = 0f;
            while (progression<Mathf.Abs(angle))
            {
                progression += Time.deltaTime*_angularVelocity*_timeMultiplier;
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
}

