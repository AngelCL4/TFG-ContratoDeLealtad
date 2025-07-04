using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GestionDeClases;

[System.Serializable]
public class Unidad
{
    public string nombre;
    public int nivel;
    public int experiencia;
    public int PV;
    public int MaxPV;
    public int poder;
    public int habilidad;
    public int velocidad;
    public int suerte;
    public int defensa;
    public int resistencia;
    public int movimiento;
    public Objeto objeto;
    public int[] probabilidadesCrecimiento;
    public Clases clase; 
    public string sprite;
    public string estado;
    public string descripcion;
    public bool bonos;
    public bool bonosPromocion;
    public string ia;
}

[System.Serializable]
public class DatosJuego
{
    public Unidad[] unidades;
}

[System.Serializable]
public class DatosEnemigos
{
    public List<Unidad> enemigos;
}

public enum TipoUso { Activo, Pasivo }

[System.Serializable]
public class Objeto 
{
    public string nombre;
    public string tipo;
    public TipoUso uso;
    public int cantidad;
    public string descripcion;
    public string spritePath;
    [System.NonSerialized] public Sprite icono;

    public int valor;
    public string statAfectada;
    public int duracion;
    public int decrementoPorTurno;
    public int rango;
    public bool afectaAliados;
}

[System.Serializable]
public class DatosObjetos
{
    public Objeto[] objetos;
}
