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

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null)
            Debug.LogError($"{gameObject.name}: Rigidbody component yok!");

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
                Random.Range(8f, 15f),
                Random.Range(-5f, 5f)
            );
            rigidBody.AddForce(force, ForceMode.Impulse);

            transform.rotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );

            rigidBody.AddTorque(
                Random.insideUnitSphere * Random.Range(500f, 1000f),
                ForceMode.Impulse
            );
        }
    }

    void Reset()
    {
        transform.position = initPosition;
        thrown = false;
        hasLanded = false;
        diceValue = 0;

        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
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

        if (diceValue == 0)
        {
            Debug.Log("Zar değeri 0 geldi, tekrar atılıyor...");
            StartCoroutine(ReRollDice(3)); // 3 deneme yap
        }
        else
        {
            GameManager.instance.ReportDiceRolled(diceValue);
        }
    }

    IEnumerator ReRollDice(int attempts)
    {
        for (int i = 0; i < attempts; i++)
        {
            Reset();
            yield return new WaitForSeconds(0.2f);

            thrown = true;
            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;

            rigidBody.AddTorque(Random.insideUnitSphere * Random.Range(500f, 1000f), ForceMode.Impulse);

            float time = 0f;
            while (!rigidBody.IsSleeping() && time < 5f)
            {
                time += Time.deltaTime;
                yield return null;
            }

            // Zar durduğunda değeri kontrol et
            hasLanded = true;
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            SideValueCheck();

            if (diceValue != 0)
                yield break; // başarılı, çık
        }

        Debug.LogWarning("Zar 3 denemede de düzgün düşmedi. 0 olarak kabul edildi.");
        GameManager.instance.ReportDiceRolled(0);
    }
}