using System;
using System.Collections;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [HideInInspector] public Direction Gravity;

    private Crystal _crystal;
    private Neighbors _neighbors;
    public Transform Position { get => gameObject.transform; }
    public bool IsEmpty { get; private set; } = false;

    public event Action EndSwapping;
    public event Action EndCheckMatching;

    public Crystal Crystal
    {
        get => _crystal;
        private set
        {
            _crystal = value;
            if (_crystal == null)
                IsEmpty = true;
            else
            {
                _crystal.ChangePositionInBoard(this);
                IsEmpty = false;
            }
        }
    }

    public void InitializeCrystal(Crystal crystal)
    {
        Crystal = crystal;
    }
    public Cell GetNeighbor(Direction direction)
    {
        switch (direction)
        {
            case Direction.Bottom:
                return _neighbors._bottom_cell;
            case Direction.Top:
                return _neighbors._top_cell;
            case Direction.Left:
                return _neighbors._left_cell;
            case Direction.Right:
                return _neighbors._right_cell;
            default:
                return null;
        }
    }

    public bool ClearCrystal()
    {
        if (Crystal != null && Crystal.MustDestroy)
        {
            DOTweenCrystalAnimService.AnimateDestroy(Crystal.gameObject, Crystal.Destroy, 0.8f);

            Crystal = null;
            return true;
        }
        return false;
    }

    public void InitialzeCell(Crystal crystal, Direction gravity, Board parent)
    {
        _crystal = crystal;
        _crystal.SubscribeIntercationAction(TrySwap);
        Gravity = gravity;
        Subscribe(parent);
    }

    public void TryMoveCrystalToEmptySpaces()
    {
        if (Crystal != null)
        {
            MoveToEmptySpace(this);
        }
    }

    public void TrySwap(Direction direction)
    {
        if (!CanSwap(direction))
        {
            Debug.Log($"Can't swap: direction {direction}");
            return;
        }
        Debug.Log($"Swap: direction {direction}");
        Cell neighbor = GetNeighbor(direction);
        if (neighbor == null)
            Debug.LogError($"The neighbor doesn't exist. Direction: {direction}");
        SwapWithNeighbor(neighbor);
    }

    public void SetNeighbors(Neighbors neighbors)
    {
        _neighbors = neighbors;
    }

    private void CheckMatch()
    {
        if (Crystal != null)
            CheckMatchByDirection(Direction.Right, GetReverseDirection(Direction.Right));
        if (Crystal != null)
            CheckMatchByDirection(Direction.Top, GetReverseDirection(Direction.Top));
        if (Crystal != null && Crystal.MustDestroy)
        {
            Crystal.MustDestroy = true;
        }

        EndCheckMatching?.Invoke();
    }

    private void CheckMatchByDirection(Direction direction, Direction directionReverse)
    {
        if (HasNeighborSameTypeCrystal(direction) && HasNeighborSameTypeCrystal(directionReverse))
        {
            Cell neighborForward = GetNeighbor(direction);
            Cell neighborBackward = GetNeighbor(directionReverse);

            Debug.Log($"Match: {Crystal.Type}");
            CheckNeighborsMatch(neighborForward, direction);
            CheckNeighborsMatch(neighborBackward, directionReverse);
            Crystal.MustDestroy = true;
        }
    }

    private void CheckNeighborsMatch(Cell cell, Direction direction)
    {
        Cell neighbor = cell?.GetNeighbor(direction);
        //until the neighboring crystal is of a different type
        if (neighbor == null || neighbor.Crystal == null || neighbor.Crystal.Type != cell.Crystal.Type)
        {
            cell.Crystal.MustDestroy = true;
            return;
        }
        Debug.Log($"Match: {neighbor.Crystal.Type}");
        CheckNeighborsMatch(neighbor, direction);
        cell.Crystal.MustDestroy = true;
    }

    private void MoveToEmptySpace(Cell cell)
    {
        Cell neighbor = cell?.GetNeighbor(Gravity);
        if (neighbor == null || !neighbor.IsEmpty)
        {
            return;
        }
        if (cell.Crystal != null)

            Debug.Log(neighbor.gameObject.name);
        Debug.Log(cell.gameObject.name);


        DebugRay(cell);
        neighbor.Crystal = cell.Crystal;
        cell.Crystal = null;
        MoveToEmptySpace(neighbor);
    }

    private void DebugRay(Cell cell)
    {
        switch (cell.Crystal.Type)
        {
            case Types.Red:
                Debug.DrawRay(cell.Crystal.transform.position, Vector3.down * 100f, Color.red, 30f);

                break;
            case Types.Blue:
                Debug.DrawRay(cell.Crystal.transform.position, Vector3.down * 100f, Color.blue, 30f);

                break;
            case Types.Green:
                Debug.DrawRay(cell.Crystal.transform.position, Vector3.down * 100f, Color.green, 30f);

                break;
            default:
                break;
        }
    }

    private void Subscribe(Board parent)
    {
        EndSwapping = parent.EndSwapping;
        EndCheckMatching = parent.CellEndCheckMaching;
        parent._startCheckingMatch += CheckMatch;
    }

    private void SwapWithNeighbor(Cell neighbor)
    {
        Crystal temporary = neighbor.Crystal;
        neighbor.Crystal = Crystal;
        Crystal = temporary;

        EndSwapping?.Invoke();
    }

    private bool CanSwap(Direction direction)
    {
        Cell neighbor = GetNeighbor(direction);
        return neighbor == null ? false : !neighbor.IsEmpty;
    }

    private bool HasNeighborSameTypeCrystal(Direction direction)
    {
        Cell neighbor = GetNeighbor(direction);
        if (neighbor == null || neighbor.Crystal == null)
            return false;
        if (neighbor.Crystal.Type != Crystal.Type)
            return false;
        return true;
    }

    private Direction GetReverseDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Bottom:
                return Direction.Top;
            case Direction.Top:
                return Direction.Bottom;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            default:
                return Direction.Left;
        }
    }

}