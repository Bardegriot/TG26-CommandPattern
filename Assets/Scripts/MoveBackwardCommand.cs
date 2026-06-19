using System.Collections;
using UnityEngine;

namespace ajc.review.playerMovement
{
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

        public IEnumerator Revert()
        {
            yield return _receiver.MoveCoroutine(Vector3.forward,_distance);
        }
    }
}