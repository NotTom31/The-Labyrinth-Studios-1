using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;

using UnityEngine.Events;

public enum CharacterType//State machine for the type of character
{
    player, enemy, npc
}
public class Character : MonoBehaviour, IMoveable, IDamageable , IKillable, IPushable
{
    //******************************************************************************************************************************************************
    //************************************************************DECLARING IMOVABLE************************************************************************
    //******************************************************************************************************************************************************
    public void Move(Rigidbody2D thisBody, Vector2 movement, float moveSpeed)
    {
        if (thisBody.CompareTag("Player"))//checks to see if this is the player moving
        {
            thisBody.MovePosition(thisBody.position + movement * moveSpeed * Time.fixedDeltaTime);//actually moves the player
            anim.SetFloat("moveX", movement.x);//allows movement animation
            anim.SetFloat("moveY", movement.y);//allows movement animation
            anim.SetBool("moving", true);//moving set true in animator
        }
        if(thisBody.CompareTag("Fighter"))//checks to see if this is an enemy moving
        {
            thisBody.MovePosition(movement);//moves the enemy
        }
    }
    //******************************************************************************************************************************************************
    //************************************************************DECLARING IDamageable*********************************************************************
    //******************************************************************************************************************************************************

    public void Damage(int damage, Collider2D obj)
    {
        GameObject temp = obj.gameObject;//reference to the gameobject attatched to obj
        GameObject.Find("Hit Sfx").GetComponent<AudioSource>().Play();//sfx for getting hit 
        if (temp.GetComponent<Character>().currentHealth > 0)//check if they still have health
        {
            temp.GetComponent<Character>().currentHealth -= damage;//compute damage
            if (temp.GetComponent<Character>().currentHealth == 0)//check if they should be dead
            { Kill(obj.gameObject); }//KILL THEM!!!!
        }            
    }
    //******************************************************************************************************************************************************
    //************************************************************DECLARING IKILLABLE***********************************************************************
    //******************************************************************************************************************************************************
    public void Kill(GameObject obj)
    {
        GameObject temp = obj;//reference to the gameobject
        if(obj.CompareTag("Player"))//check if it is the player that has died
        {
           temp.GetComponent<Player>().PlayerDied.Invoke();//kills player
        }
        else { Destroy(obj); }//kills everything else   
    }
    //******************************************************************************************************************************************************
    //************************************************************DECLARING IPUSHABLE***********************************************************************
    //******************************************************************************************************************************************************
    public float thrust { get;  set; }//how hard an object is pushed back
    public float pushTime { get; set; }//how long the push back last
    public void Push(Collider2D obj)
    {        
            Rigidbody2D character = obj.GetComponent<Rigidbody2D>();//reference to the Rigidbody component
            if (character != null)//makes sure the object hasn't already been destroyed
            {
                Vector2 difference = character.transform.position - transform.position;//not sure lol
                difference = difference.normalized * thrust;//this is where the thrust will change how far something is pushed back
                character.AddForce(difference, ForceMode2D.Impulse);//the actual push occurs here
                StartCoroutine(PushCo(character));
            }
        if (obj.CompareTag("Player"))//Player damaged, will  run the blink routine
        {
            GameObject temp = obj.gameObject;//reference to the gameobject attatched to the rigidbody
            StartCoroutine(temp.GetComponent<Player>().playerBlink(temp));//start coroutine
        }
    }
    public IEnumerator PushCo(Rigidbody2D character)
    {
        if (character != null)
        {
            yield return new WaitForSeconds(pushTime);
            character.velocity = Vector2.zero;
        }
    }
    //******************************************************************************************************************************************************
    //********************************************************CHARACTER CLASS ATTRIBUTES********************************************************************
    //******************************************************************************************************************************************************
    public Rigidbody2D thisBody;
    protected Animator anim;
    protected Vector2 movement;
    public float moveSpeed;
    public CharacterType charType;
    public int attackDamage;
    public int currentHealth;
    //******************************************************************************************************************************************************
    //*****************************************************************ATTACKING TRIGGERED******************************************************************
    //******************************************************************************************************************************************************

    private void OnTriggerEnter2D(Collider2D obj)
    {
        if (obj.CompareTag("Player") || obj.CompareTag("Fighter"))
        {
            Push(obj);
            Damage(attackDamage, obj);
        }
    }
}