using System.Collections;

namespace ajc.review.playerMovement
{
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

        public IEnumerator Revert()
        {
            yield return _target.RotateCoroutine(-_angle);
        }
    }
}