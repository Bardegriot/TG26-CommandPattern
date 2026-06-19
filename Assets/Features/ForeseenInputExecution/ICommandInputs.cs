using UnityEngine;

public interface ICommandInputs
{
    #region Publics

    public void AddToInputQueue();
    public void Execute(RobotController robotCommand);

    #endregion

}

public interface IMoveForwardCommandReceiver : ICommandInputs
{
    #region Publics


    public void MoveForward();


    #endregion

}
public interface IRotateCommandReceiver : ICommandInputs
{
    #region Publics

    public void RotateCommand();


    #endregion

}

public class MoveForwardCommand : ICommandInputs
{
    public void AddToInputQueue() {}

    public void Execute(RobotController robotCommand)
    {
        robotCommand.MoveForward();
    }
}
public class RotateCommand : ICommandInputs
{
    public void AddToInputQueue() { }

    public RotateCommand(float angle)
    {
        _angle = angle;
    }

    public void Execute(RobotController robotCommand)
    {
        robotCommand.Rotate(_angle);
    }

    private float _angle;
}
