using CatOnTower;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OutWay : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("CELL"))
        {
            StartCoroutine(LevelManager.instance.CheckForLevelCompletion());
           StartCoroutine(DropCells(other.gameObject));
        }
       
    }

    IEnumerator DropCells(GameObject cell)
    {
        yield return new WaitForSeconds(0.15f);
        if (cell != null)
        {
            cell.transform.DOLocalMoveY(-20, 1.8f);
            Destroy(cell, 1f);
        }
    }
}
