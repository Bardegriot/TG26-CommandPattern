using System.Collections;
using UnityEngine;

namespace ajc.review.playerMovement
{
    public interface IMoveCommandReceiver
    {
        IEnumerator MoveCoroutine(Vector3 direction, float distance);
    }
}