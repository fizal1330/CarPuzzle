using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Cell : MonoBehaviour
{
    public enum Type {CAR,PAINTSHOP,GARRAGE,WALL,EMPTY}
    public Type currentType;

    private Vector3 originalPosition;

    private void Start()
    {

        //if(currentType != Type.none)
        //{
        //    originalPosition = transform.position;

        //    transform.localScale= new Vector3(0.1f,0.1f,0.1f);

        //    MoveCube();
        //    ScaleUp();
        //}

    }

    private void ScaleUp()
    {
        transform.DOScale(new Vector3(1, 0.25f, 1), 0.3f);
    }

    private void MoveCube()
    {
        // Specify the move duration
        float moveDuration = 0.3f;

        // Specify the wait duration in the elevated position
        float waitDuration = 0.4f;

        // Sequence the movements using DoTween
        Sequence sequence = DOTween.Sequence();

        // Move up from the original Y-axis position
        sequence.Append(transform.DOMoveY(originalPosition.y + 1f, moveDuration));

        // Wait in the elevated position
        sequence.AppendInterval(waitDuration);

        // Move down to the original Y-axis position
        sequence.Append(transform.DOMove(originalPosition, moveDuration));
    }
}
