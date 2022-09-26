using UnityEngine;

public class ThinkingPlaceable : Placeable
{
    [HideInInspector] public States state = States.Dragged;
    public enum States
    {
        Dragged, //플레이어가 플레이 필드에서 카드로 끌 때
        Idle, //맨 처음에, 떨어졌을 때.
    }
}
