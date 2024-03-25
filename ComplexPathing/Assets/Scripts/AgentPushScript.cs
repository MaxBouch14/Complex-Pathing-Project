using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentPushScript : MonoBehaviour
{
    //In an effort to prevent overlap, this functions gets the "depth" of the collision of two agents using the fact that they are capsules and have a radius.
    //It will push agents away from each other while not preventing travelling on a path.

    //Unfortunately, rarely collisions are ignored, and this method is not perfect - it may sometimes make it seem like agents are floating when pushed off an edge, 
    //And may also make it seem like two agents are walking side-by-side on a bridge, though the underlying bridge logic is still sound - there are never agents walking abreast.
    //I did not have enough time to fix these issues.

    //These two functions are identical.
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Agent")
        {
            float amtToMove = (collider.gameObject.GetComponent<CapsuleCollider>().radius + gameObject.GetComponent<CapsuleCollider>().radius) - (gameObject.transform.position - collider.gameObject.transform.position).magnitude;
            if(amtToMove == 0){
                amtToMove = 0.5f;
            }
            //Due to agents collider radii being slightly bigger than the render, we decrease amtToMove slightly so they aren't pushed as ridiculously far away from eachother.
            Vector3 curPosThis = Vector3.MoveTowards(gameObject.transform.position, collider.gameObject.transform.position, - (0.3f * amtToMove));
            Vector3 curPosCollider = Vector3.MoveTowards(collider.gameObject.transform.position, gameObject.transform.position, - ( 0.3f * amtToMove));
            gameObject.GetComponent<Rigidbody>().MovePosition(curPosThis);
            collider.gameObject.GetComponent<Rigidbody>().MovePosition(curPosCollider);
        }else if (collider.gameObject.tag == "Obstacle"){
            Vector3 curPosThis = Vector3.MoveTowards(gameObject.transform.position, collider.gameObject.transform.position, - (0.1f));
            gameObject.GetComponent<Rigidbody>().MovePosition(curPosThis);
        }
    }

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.tag == "Agent")
        {
            float amtToMove = (collider.gameObject.GetComponent<CapsuleCollider>().radius + gameObject.GetComponent<CapsuleCollider>().radius) - (gameObject.transform.position - collider.gameObject.transform.position).magnitude;
            if(amtToMove == 0){
                amtToMove = 0.5f;
            }
            Vector3 curPosThis = Vector3.MoveTowards(gameObject.transform.position, collider.gameObject.transform.position, - (0.5f * amtToMove));
            Vector3 curPosCollider = Vector3.MoveTowards(collider.gameObject.transform.position, gameObject.transform.position, - ( 0.5f * amtToMove));
            gameObject.GetComponent<Rigidbody>().MovePosition(curPosThis);
            collider.gameObject.GetComponent<Rigidbody>().MovePosition(curPosCollider);
        }else if (collider.gameObject.tag == "Obstacle"){
            Vector3 curPosThis = Vector3.MoveTowards(gameObject.transform.position, collider.gameObject.transform.position, - (0.1f));
            gameObject.GetComponent<Rigidbody>().MovePosition(curPosThis);
        }
    }
}
