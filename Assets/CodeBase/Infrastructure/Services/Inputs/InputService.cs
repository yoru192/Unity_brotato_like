using UnityEngine;

namespace CodeBase.Infrastructure.Services.Inputs
{
    public abstract class InputService : IInputService
    {
        protected const string Horizontal = "Horizontal";
        protected const string Vertical = "Vertical";
        
        public abstract Vector2 AxisRaw { get; }
        
        protected static Vector2 InputAxisRaw() => 
            new Vector2(Input.GetAxisRaw(Horizontal), Input.GetAxisRaw(Vertical));
    }
}