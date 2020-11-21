namespace Game
{
    class FreeController : ControllerComponent
    {
        public override float XAxis => _xAxis;
        float _xAxis;
        public void SetXAxis(float xAxis) => _xAxis = xAxis;
        public override bool JumpPressed => _jumpPressed;
        bool _jumpPressed;
        public void SetJumpPressed(bool jumpPressed) => _jumpPressed = jumpPressed;
        public override bool JumpDown => _jumpDown;
        bool _jumpDown;
        public void SetJumpDown(bool jumpDown) => _jumpDown = jumpDown;
        public override bool AttackPressed => _attackPressed;
        bool _attackPressed;
        public void SetAttackPressed(bool attackPressed) => _attackPressed = attackPressed;
        public override bool InteractPressed => _interactPressed;
        bool _interactPressed;
        public void SetInteractPressed(bool interactPressed) => _interactPressed = interactPressed;
    }
}
