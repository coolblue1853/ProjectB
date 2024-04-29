using UnityEngine;
using DG.Tweening;
namespace BehaviorDesigner.Runtime
{
    // Wrapper for the Behavior class
    [AddComponentMenu("Behavior Designer/Behavior Tree")]
    public class BehaviorTree : Behavior
    {
        public bool isJumping = false;
        public Sequence sequence;
        // intentionally left blank
    }
}