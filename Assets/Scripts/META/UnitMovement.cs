using System;
using System.Collections;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.AI;


public class UnitMovement : MonoBehaviour {
    Camera cam;
    NavMeshAgent agent;
    public LayerMask ground; //drag and drop in the object from the hierarchy to set object as ground

    public bool isCommandedToMove;

    DirectionIndicator directionIndicator;

    private void Start() {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();

        directionIndicator = GetComponent<DirectionIndicator>();

    }

    private void Update() {
        if (Input.GetMouseButtonDown(1) && IsMovingPossible()) {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground)) {
                isCommandedToMove = true;
                StartCoroutine(NoCommand()); // will use the IEnumerator method that will wait for a second before setting isCommandedToMove to false 

                agent.SetDestination(hit.point);
                SoundManager.Instance.PlayUnitCommandSound();
                directionIndicator.DrawLine(hit);

            }
        }

        // // Agent has reached destination
        // if (agent.hasPath == false || agent.remainingDistance <= agent.stoppingDistance) {
        //     isCommandedToMove = false;

        // }

        IEnumerator NoCommand() {
            yield return new WaitForSeconds(1);
            isCommandedToMove = false;
        }


    }

    private bool IsMovingPossible() {
        return CursorManager.Instance.currentCursor != CursorManager.CursorType.UnAvailable;
    }
}
