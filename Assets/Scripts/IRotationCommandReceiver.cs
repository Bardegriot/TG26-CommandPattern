using System.Collections;

namespace ajc.review.playerMovement
{
    public interface IRotationCommandReceiver
    {
        IEnumerator RotateCoroutine(float angle);
    }
}