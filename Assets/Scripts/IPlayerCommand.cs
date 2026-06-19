using System.Collections;

namespace ajc.review.playerMovement
{
    public interface IPlayerCommand
    {
        IEnumerator Execute();
        IEnumerator Revert();
    }
}