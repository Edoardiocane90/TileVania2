using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class PowerUp : RewardItem
{
    public float DefenseUp { get; set; }

    public float DefenseMultiplier { get; set; }

    public float AttackUp { get; set; }

    public float AttackMultiplier { get; set; }

    public GameObject Projectile { get; set; }

    public int LivesUp { get; set; }

    public string Description { get; set; }
}
