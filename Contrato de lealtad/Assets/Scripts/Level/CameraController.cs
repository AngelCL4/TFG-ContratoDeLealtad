using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public Transform pointer;
    public Tilemap tilemap;
    public Camera cam;
    private Vector3 minBounds, maxBounds;
    private float camHeight, camWidth;

    private void Start()
    {
        if (tilemap == null || cam == null)
        {
            Debug.LogError("Tilemap o Cámara no asignados.");
            return;
        }

        // Límites del tilemap
        BoundsInt bounds = tilemap.cellBounds;
        minBounds = tilemap.CellToWorld(bounds.min);
        maxBounds = tilemap.CellToWorld(bounds.max);

        // Calculo del tamaño de la cámara
        camHeight = cam.orthographicSize * 2;
        camWidth = camHeight * cam.aspect;

        // Si el tilemap es menor, ajustar el tamaño de la cámara
        float mapWidth = (maxBounds.x - minBounds.x);
        float mapHeight = (maxBounds.y - minBounds.y);

        if (mapWidth < camWidth)
            cam.orthographicSize = mapWidth / (2 * cam.aspect);

        if (mapHeight < camHeight)
            cam.orthographicSize = mapHeight / 2;

        float maxTileHeight = Mathf.Max(mapWidth, mapHeight);
        if (maxTileHeight < camHeight)
        {
            cam.orthographicSize = Mathf.Max(mapWidth, mapHeight) / 2;
        }
    }

    private void LateUpdate()
    {
        if (pointer == null) return;

        // Hacer que la cámara siga al puntero
        Vector3 newPos = pointer.position;
        newPos.z = cam.transform.position.z;

        // Restringir dentro de los límites del Tilemap
        float halfWidth = cam.orthographicSize * cam.aspect;
        float halfHeight = cam.orthographicSize;

        newPos.x = Mathf.Clamp(newPos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
        newPos.y = Mathf.Clamp(newPos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

        cam.transform.position = newPos;
    }
}