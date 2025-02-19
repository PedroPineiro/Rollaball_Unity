using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostRampa : MonoBehaviour {
    public float boostForce = 10f; // Fuerza del impulso
    public Vector3 boostDirection = new Vector3(0, 1, 1); // Dirección del impulso

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) { // Asegura que solo el jugador sea impulsado
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.AddForce(boostDirection.normalized * boostForce, ForceMode.Impulse);
            }
        }
    }
}