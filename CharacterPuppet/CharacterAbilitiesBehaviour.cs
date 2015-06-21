using UnityEngine;
using System.Collections;

namespace JamTools
{
    //This behaviour implements an interface
    //CharacterPuppet derives from this class
    //Control scripts can point to a CharacterPuppet or another abilities behaviour in between
    public class CharacterAbilitiesBehaviour : MonoBehaviour,  CharacterAbilities
    {
        #region CharacterAbilities implementation

        public virtual void Jump()
        {
            throw new System.NotImplementedException();
        }

        public virtual void MoveInput(Vector3 direction)
        {
            throw new System.NotImplementedException();
        }

        public virtual void MoveTarget(Vector3? position)
        {
            throw new System.NotImplementedException();
        }

        public virtual bool CanJump()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool WaitingOnJumpDelay()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsGrounded()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
