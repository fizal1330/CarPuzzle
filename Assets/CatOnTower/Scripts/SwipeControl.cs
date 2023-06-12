using UnityEngine;

namespace CatOnTower
{
    public enum Swipe { None, Up, Down, Left, TopLeft, BottomLeft, Right, TopRight, BottomRight };

    public class SwipeControl
    {
        private Vector2 startPos;
        private Vector2 endPos;
        private LevelManager _levelManager;
        public bool canDetectCube = true;
        public int swipeForce;

        public void SetLevelManager(LevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        public void OnUpdate()
        {

            if (Input.touchCount > 0)
            {
                Touch fingerTouch = Input.GetTouch(0);
                if (fingerTouch.phase == TouchPhase.Began)
                {
                    startPos = fingerTouch.position;
                    endPos = startPos;
                    if (canDetectCube)
                    {
                        DetectCube();
                    }
                }

                if (fingerTouch.phase == TouchPhase.Ended)
                {
                    endPos = fingerTouch.position;
                    if (Vector2.Distance(endPos, startPos) > 0.1f)
                    {
                        CalculateSwipeForce();
                        SwipeDirection();
                        canDetectCube = true;
                    }
                }
            }
        }

        private void CalculateSwipeForce()
        {
            // Calculate the distance between start and end positions
            float swipeDistance = (endPos - startPos).magnitude;

            // Calculate the swipe force based on the distance
            float force = swipeDistance / Time.deltaTime;

            // Adjust the speed of the object using the swipe force
            // You can modify this based on your specific implementation
            float speedMultiplier = 1f; // Adjust this value to control the speed change
            swipeForce =  1 * (int) speedMultiplier * (int) force /1000;

           // Debug.LogError("FORCE  - " + swipeForce);
        }

        void DetectCube()
        {
            Touch fingerTouch = Input.GetTouch(0);
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(fingerTouch.position);

            Physics.Raycast(ray, out hit);
            
            if(hit.collider != null && hit.collider.CompareTag("CELL"))
            {
                //assigns that to the current cell
                _levelManager.currentCell = hit.collider.gameObject.GetComponent<Cell>();
                canDetectCube = false;
            }
            else
            {
                return;
            }
           
        }

        private Swipe SwipeDirection()
        {         
            Swipe direction = Swipe.None;
            Vector2 currentSwipe = endPos - startPos;
            //calculates the angle
            float angle = ((Mathf.Atan2(currentSwipe.y, currentSwipe.x) * (180 / Mathf.PI)));

            if (angle > 45 && angle < 135)
            {
                direction = Swipe.Up;
            }
            else if (angle < -45 && angle > -135)
            {
                direction = Swipe.Down;
            }
            else if (angle < -135 || angle > 135)
            {
                direction = Swipe.Left;
            }
            else if (angle > -45 && angle < 45)
            {
                direction = Swipe.Right;
            }      
           // Checks for null and calls MoveCube function
            if(_levelManager.currentCell != null)
            {
                //Gets the index of current selected cell
                Vector2Int index = _levelManager.WorldToGridIndex(_levelManager.currentCell.gameObject.transform.localPosition, _levelManager.grid._cellSize, Vector3.zero);

                if (direction != Swipe.None)
                {
                    _levelManager.MoveCube(direction, index);
                    direction = Swipe.None;
                }
            }
                  
            return direction;
        }
    }
}