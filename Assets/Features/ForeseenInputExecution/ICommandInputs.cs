using UnityEngine;

public interface ICommandInputs
{
    #region Publics

    public void AddToInputQueue();
    public void Execute(RobotController robotExecute);

    public void UndoCommand(RobotController robotUndo);
    public void RedoCommand(RobotController robotRedo);

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
public interface IUndoCommand : ICommandInputs
{
    #region Publics

    public void UndoCommand();

    #endregion

}

public class MoveForwardCommand : ICommandInputs
{
    public void AddToInputQueue() {}

    public void Execute(RobotController robotCommand)
    {
        robotCommand.MoveForward();
    }

    public void UndoCommand(RobotController robotUndo)
    {

    }
    public void RedoCommand(RobotController robotRedo)
    {

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

    public void UndoCommand(RobotController robotUndo)
    {

    }
    public void RedoCommand(RobotController robotRedo)
    {

    }

    private float _angle;
}

