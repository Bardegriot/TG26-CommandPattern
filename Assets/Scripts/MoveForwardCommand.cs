using System.Collections;
using UnityEngine;

namespace ajc.review.playerMovement
{
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

        public IEnumerator Revert()
        {
            yield return _target.MoveCoroutine(Vector3.back,_distance);
        }
    }
}