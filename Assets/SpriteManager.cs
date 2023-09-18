using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;

    private static SpriteManager instance;

    public static SpriteManager Instance { get => instance; }
    public Sprite[] Sprites { get => sprites; set => sprites = value; }

    private void Start()
    {
        if(Instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}
