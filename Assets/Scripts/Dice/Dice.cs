using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    Rigidbody rigidBody;
    bool hasLanded;
    bool thrown;

    Vector3 initPosition;
    int diceValue;

    [SerializeField] DiceSide[] diceSides;

    void Start()
    {
        
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null)
            Debug.LogError($"{gameObject.name}: Rigidbody component yok!");

        //rigidBody = GetComponent<Rigidbody>();
        initPosition = transform.position;
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
    }

    void Update()
    {
        if (rigidBody.IsSleeping() && !hasLanded && thrown)
        {
            hasLanded = true;
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            SideValueCheck();
        }
        else if (rigidBody.IsSleeping() && hasLanded && diceValue == 0)
        {
            ReRollDice();
        }
    }

    public void RollDice()
    {
        Reset();
        if (!thrown && !hasLanded)
        {
            thrown = true;
            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;

            Vector3 force = new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(8f, 15f),  // YUKARI DOĞRU KUVVET UYGULAYARAK HAVADA RASTGELE DÖNMESİNİ SAĞLA
                Random.Range(-5f, 5f)
            );
            rigidBody.AddForce(force, ForceMode.Impulse);

            transform.rotation = Quaternion.Euler( // RASTGELE DÖNDÜR
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
            );

            rigidBody.AddTorque(
                Random.insideUnitSphere * Random.Range(500f, 1000f),
                ForceMode.Impulse
            );

        }
        // else if (thrown && hasLanded)
        // {
        //     // ZARI SIFIRLA
        //     Reset();
        // }
    }

    void Reset()
    {
        transform.position = initPosition;
        thrown = false;
        hasLanded = false;

        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
    }

    void ReRollDice()
    {
        Reset();

        thrown = true;
        rigidBody.useGravity = true;
        rigidBody.isKinematic = false;
        rigidBody.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
    }

    void SideValueCheck()
    {
        diceValue = 0;
        foreach (var side in diceSides)
        {
            if (side.OnGround)
            {
                diceValue = side.SideValue();
                break;
            }
        }
        GameManager.instance.ReportDiceRolled(diceValue);
    }
}
