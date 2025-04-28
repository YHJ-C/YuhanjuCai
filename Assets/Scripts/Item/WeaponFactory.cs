using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class WeaponFactory
{
    private static readonly Dictionary<string, Type> weaponTypes = new Dictionary<string, Type>();

    static WeaponFactory()
    {
        // 注册所有武器类型
        foreach (var type in typeof(WeaponBase).Assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(WeaponBase)) && !type.IsAbstract)
            {
                weaponTypes[type.Name] = type;
            }
        }
    }

    public static WeaponBase CreateWeapon(string weaponName)
    {        
        if (weaponTypes.TryGetValue(weaponName, out var type))
        {
            return (WeaponBase)Activator.CreateInstance(type);
        }
        throw new ArgumentException($"Weapon type '{weaponName}' not found.");
    }
}
