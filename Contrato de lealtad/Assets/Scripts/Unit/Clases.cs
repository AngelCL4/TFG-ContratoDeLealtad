using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GestionDeClases
{
    [System.Serializable]
    public class Clases
    {
        public string nombre;
        public string tipoMovimiento;
        public string tipoDano;
        public int rangoAtaqueMinimo;
        public int rangoAtaqueMaximo;
        public string tier;
        public int bonusPV;
        public int bonusPoder;
        public int bonusHabilidad;
        public int bonusVelocidad;
        public int bonusSuerte;
        public int bonusDefensa;
        public int bonusResistencia;
        public int bonusMovimiento;
    }

    public static class GestorDeClases{ 
        // Lista de clases disponibles
        public static List<Clases> clasesDisponibles = new List<Clases>()
        {
            new Clases { nombre = "Lord", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 2, bonusPoder = 2, bonusHabilidad = 1, bonusVelocidad = 1, bonusSuerte = 1, bonusDefensa = 1, bonusResistencia = 0, bonusMovimiento = 0 },
            new Clases { nombre = "Adalid", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 1, bonusPoder = 2, bonusHabilidad = 1, bonusVelocidad = 1, bonusSuerte = 2, bonusDefensa = 1, bonusResistencia = 1, bonusMovimiento = 1 },
            new Clases { nombre = "Curandero", tipoMovimiento = "Pie", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Basica", bonusPV = 2, bonusPoder = 1, bonusHabilidad = 0, bonusVelocidad = 1, bonusSuerte = 2, bonusDefensa = 0, bonusResistencia = 2, bonusMovimiento = 0 },
            new Clases { nombre = "Clerigo", tipoMovimiento = "Pie", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Avanzada", bonusPV = 1, bonusPoder = 1, bonusHabilidad = 0, bonusVelocidad = 2, bonusSuerte = 3, bonusDefensa = 0, bonusResistencia = 2, bonusMovimiento = 1 },
            new Clases { nombre = "Mago", tipoMovimiento = "Pie", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Basica", bonusPV = 1, bonusPoder = 3, bonusHabilidad = 1, bonusVelocidad = 1, bonusSuerte = 0, bonusDefensa = 0, bonusResistencia = 2, bonusMovimiento = 0 },
            new Clases { nombre = "Sabio", tipoMovimiento = "Pie", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Avanzada", bonusPV = 1, bonusPoder = 3, bonusHabilidad = 1, bonusVelocidad = 2, bonusSuerte = 1, bonusDefensa = 0, bonusResistencia = 1, bonusMovimiento = 1 },
            new Clases { nombre = "Luchador", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 2, bonusPoder = 1, bonusHabilidad = 1, bonusVelocidad = 1, bonusSuerte = 1, bonusDefensa = 1, bonusResistencia = 1, bonusMovimiento = 0 },
            new Clases { nombre = "Heroe", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 2, bonusPoder = 1, bonusHabilidad = 1, bonusVelocidad = 1, bonusSuerte = 2, bonusDefensa = 1, bonusResistencia = 1, bonusMovimiento = 1 },
            new Clases { nombre = "Arquero", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 2, rangoAtaqueMaximo = 2, tier = "Basica", bonusPV = 1, bonusPoder = 1, bonusHabilidad = 3, bonusVelocidad = 2, bonusSuerte = 1, bonusDefensa = 0, bonusResistencia = 0, bonusMovimiento = 0 },
            new Clases { nombre = "Tirador", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 2, rangoAtaqueMaximo = 2, tier = "Avanzada", bonusPV = 2, bonusPoder = 1, bonusHabilidad = 3, bonusVelocidad = 2, bonusSuerte = 1, bonusDefensa = 0, bonusResistencia = 0, bonusMovimiento = 1 },
            new Clases { nombre = "Hachero", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 2, bonusPoder = 3, bonusHabilidad = 0, bonusVelocidad = 1, bonusSuerte = 1, bonusDefensa = 1, bonusResistencia = 0, bonusMovimiento = 0 },
            new Clases { nombre = "Berserker", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 3, bonusPoder = 3, bonusHabilidad = 0, bonusVelocidad = 2, bonusSuerte = 0, bonusDefensa = 1, bonusResistencia = 0, bonusMovimiento = 1 },
            new Clases { nombre = "Soldado", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 2, bonusPoder = 1, bonusHabilidad = 2, bonusVelocidad = 0, bonusSuerte = 0, bonusDefensa = 2, bonusResistencia = 1, bonusMovimiento = 0 },
            new Clases { nombre = "Guerrillero", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 2, bonusPoder = 2, bonusHabilidad = 3, bonusVelocidad = 0, bonusSuerte = 0, bonusDefensa = 1, bonusResistencia = 1, bonusMovimiento = 1 },
            new Clases { nombre = "Mago Oscuro", tipoMovimiento = "Pie", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Basica", bonusPV = 1, bonusPoder = 2, bonusHabilidad = 2, bonusVelocidad = 0, bonusSuerte = 1, bonusDefensa = 0, bonusResistencia = 2, bonusMovimiento = 0 },
            new Clases { nombre = "Hechicero", tipoMovimiento = "Pie", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Avanzada", bonusPV = 1, bonusPoder = 2, bonusHabilidad = 1, bonusVelocidad = 0, bonusSuerte = 2, bonusDefensa = 0, bonusResistencia = 3, bonusMovimiento = 1 },
            new Clases { nombre = "Adivino", tipoMovimiento = "Pie", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Basica", bonusPV = 1, bonusPoder = 0, bonusHabilidad = 3, bonusVelocidad = 1, bonusSuerte = 2, bonusDefensa = 0, bonusResistencia = 1, bonusMovimiento = 0 },
            new Clases { nombre = "Medium", tipoMovimiento = "Pie", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Avanzada", bonusPV = 1, bonusPoder = 1, bonusHabilidad = 3, bonusVelocidad = 1, bonusSuerte = 2, bonusDefensa = 0, bonusResistencia = 1, bonusMovimiento = 1 },
            new Clases { nombre = "Barbaro", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 2, bonusPoder = 2, bonusHabilidad = 0, bonusVelocidad = 0, bonusSuerte = 2, bonusDefensa = 0, bonusResistencia = 2, bonusMovimiento = 0 },
            new Clases { nombre = "Cacique", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 2, bonusPoder = 1, bonusHabilidad = 1, bonusVelocidad = 0, bonusSuerte = 1, bonusDefensa = 1, bonusResistencia = 3, bonusMovimiento = 1 },
            new Clases { nombre = "Cazador", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 2, rangoAtaqueMaximo = 2, tier = "Basica", bonusPV = 1, bonusPoder = 2, bonusHabilidad = 1, bonusVelocidad = 3, bonusSuerte = 0, bonusDefensa = 1, bonusResistencia = 0, bonusMovimiento = 0 },
            new Clases { nombre = "Caballero Arco", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 2, rangoAtaqueMaximo = 2, tier = "Avanzada", bonusPV = 1, bonusPoder = 2, bonusHabilidad = 1, bonusVelocidad = 2, bonusSuerte = 0, bonusDefensa = 1, bonusResistencia = 0, bonusMovimiento = 3 },
            new Clases { nombre = "Esgrimista", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 0, bonusPoder = 2, bonusHabilidad = 2, bonusVelocidad = 2, bonusSuerte = 0, bonusDefensa = 1, bonusResistencia = 1, bonusMovimiento = 0 },
            new Clases { nombre = "Duelista", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 1, bonusPoder = 2, bonusHabilidad = 2, bonusVelocidad = 2, bonusSuerte = 0, bonusDefensa = 1, bonusResistencia = 1, bonusMovimiento = 1 },
            new Clases { nombre = "Estratega", tipoMovimiento = "Pie", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Basica", bonusPV = 1, bonusPoder = 1, bonusHabilidad = 2, bonusVelocidad = 1, bonusSuerte = 1, bonusDefensa = 2, bonusResistencia = 0, bonusMovimiento = 0 },
            new Clases { nombre = "Maestro", tipoMovimiento = "Pie", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Avanzada", bonusPV = 1, bonusPoder = 1, bonusHabilidad = 2, bonusVelocidad = 0, bonusSuerte = 1, bonusDefensa = 3, bonusResistencia = 1, bonusMovimiento = 1 },
            new Clases { nombre = "Infanteria Pesada", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 3, bonusPoder = 2, bonusHabilidad = 0, bonusVelocidad = 0, bonusSuerte = 0, bonusDefensa = 3, bonusResistencia = 0, bonusMovimiento = 0 },
            new Clases { nombre = "General", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 4, bonusPoder = 2, bonusHabilidad = 0, bonusVelocidad = 0, bonusSuerte = 0, bonusDefensa = 4, bonusResistencia = 0, bonusMovimiento = 0 },
            new Clases { nombre = "Jinete Dragon", tipoMovimiento = "Volador", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 1, bonusPoder = 2, bonusHabilidad = 1, bonusVelocidad = 0, bonusSuerte = 0, bonusDefensa = 2, bonusResistencia = 0, bonusMovimiento = 2 },
            new Clases { nombre = "Caballero Dragon", tipoMovimiento = "Volador", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 2, bonusPoder = 3, bonusHabilidad = 1, bonusVelocidad = 0, bonusSuerte = 0, bonusDefensa = 2, bonusResistencia = 1, bonusMovimiento = 1 },
            new Clases { nombre = "Jinete", tipoMovimiento = "Montado", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 1, bonusPoder = 1, bonusHabilidad = 1, bonusVelocidad = 1, bonusSuerte = 0, bonusDefensa = 1, bonusResistencia = 1, bonusMovimiento = 2 },
            new Clases { nombre = "Paladin", tipoMovimiento = "Montado", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 2, bonusPoder = 2, bonusHabilidad = 1, bonusVelocidad = 1, bonusSuerte = 1, bonusDefensa = 1, bonusResistencia = 1, bonusMovimiento = 1 },
            new Clases { nombre = "Jinete Pegaso", tipoMovimiento = "Volador", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 0, bonusPoder = 0, bonusHabilidad = 2, bonusVelocidad = 3, bonusSuerte = 0, bonusDefensa = 0, bonusResistencia = 1, bonusMovimiento = 2 },
            new Clases { nombre = "Caballero Pegaso", tipoMovimiento = "Volador", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 1, bonusPoder = 1, bonusHabilidad = 1, bonusVelocidad = 3, bonusSuerte = 0, bonusDefensa = 0, bonusResistencia = 2, bonusMovimiento = 1 },
            new Clases { nombre = "Sicario", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 1, bonusPoder = 1, bonusHabilidad = 2, bonusVelocidad = 2, bonusSuerte = 2, bonusDefensa = 0, bonusResistencia = 0, bonusMovimiento = 0 },
            new Clases { nombre = "Asesino", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 1, bonusPoder = 1, bonusHabilidad = 3, bonusVelocidad = 2, bonusSuerte = 2, bonusDefensa = 0, bonusResistencia = 0, bonusMovimiento = 1 },
            new Clases { nombre = "Mirmidon", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Basica", bonusPV = 1, bonusPoder = 0, bonusHabilidad = 2, bonusVelocidad = 3, bonusSuerte = 1, bonusDefensa = 0, bonusResistencia = 1, bonusMovimiento = 0 },
            new Clases { nombre = "Espadachin", tipoMovimiento = "Pie", tipoDano = "Fisico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 1, tier = "Avanzada", bonusPV = 0, bonusPoder = 1, bonusHabilidad = 3, bonusVelocidad = 4, bonusSuerte = 1, bonusDefensa = 0, bonusResistencia = 0, bonusMovimiento = 1 },
            new Clases { nombre = "Trovador", tipoMovimiento = "Montado", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Basica", bonusPV = 2, bonusPoder = 0, bonusHabilidad = 0, bonusVelocidad = 0, bonusSuerte = 1, bonusDefensa = 1, bonusResistencia = 2, bonusMovimiento = 2 },
            new Clases { nombre = "Caballero Mago", tipoMovimiento = "Montado", tipoDano = "Magico", rangoAtaqueMinimo = 1, rangoAtaqueMaximo = 2, tier = "Avanzada", bonusPV = 3, bonusPoder = 1, bonusHabilidad = 0, bonusVelocidad = 0, bonusSuerte = 2, bonusDefensa = 1, bonusResistencia = 2, bonusMovimiento = 1 },
        }; 
    }
}


