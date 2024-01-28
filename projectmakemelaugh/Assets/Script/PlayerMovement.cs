using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{

	public PlayerData Data;
	public Animator animator;
	public float init_force;
	public float init_speed;
	public float heightplus = 5;
	public float speedplus = 11;
	public bool goofy = true;
	public int E_goofy=0;
	public int E_ngoofy=0;
	public int Q_ngoofy=0;
	public int Q_goofy = 0;

	public Slider goofyswitch;
	public TextMeshProUGUI goofy_text;
	#region Variables

	public Rigidbody2D RB { get; private set; }

	public bool IsFacingRight { get; private set; }
	public bool IsJumping { get; private set; }
	public bool IsWallJumping { get; private set; }
	public bool IsSliding { get; private set; }


	public float LastOnGroundTime { get; private set; }
	public float LastOnWallTime { get; private set; }
	public float LastOnWallRightTime { get; private set; }
	public float LastOnWallLeftTime { get; private set; }


	private bool _isJumpCut;
	private bool _isJumpFalling;


	private float _wallJumpStartTime;
	private int _lastWallJumpDir;

	private Vector2 _moveInput;
	public float LastPressedJumpTime { get; private set; }


	[Header("Checks")]
	[SerializeField] private Transform _groundCheckPoint;

	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
	[Space(5)]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
	[SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
	#endregion

	private void Awake()
	{
		RB = GetComponent<Rigidbody2D>();
	}

	private void Start()
	{
		SetGravityScale(Data.gravityScale);
		init_force = Data.jumpForce;
		init_speed = Data.runMaxSpeed;
		IsFacingRight = true;
	}

	private void Update()
	{
		#region TIMERS
		LastOnGroundTime -= Time.deltaTime;
		LastOnWallTime -= Time.deltaTime;
		LastOnWallRightTime -= Time.deltaTime;
		LastOnWallLeftTime -= Time.deltaTime;

		LastPressedJumpTime -= Time.deltaTime;
		#endregion

		#region INPUT HANDLER
		_moveInput.x = Input.GetAxisRaw("Horizontal");
		_moveInput.y = Input.GetAxisRaw("Vertical");

		if (_moveInput.x != 0)
			CheckDirectionToFace(_moveInput.x > 0);

		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J))
		{
			OnJumpInput();
		}

		if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J))
		{
			OnJumpUpInput();
		}
		#endregion

		#region COLLISION CHECKS
		if (!IsJumping)
		{

			if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping) 
			{
				LastOnGroundTime = Data.coyoteTime; 
			}


			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
					|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
				LastOnWallRightTime = Data.coyoteTime;


			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
				LastOnWallLeftTime = Data.coyoteTime;


			LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
		}
		#endregion

		#region JUMP CHECKS
		if (IsJumping && RB.velocity.y < 0)
		{
			IsJumping = false;

			if (!IsWallJumping)
				_isJumpFalling = true;
		}

		if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
		{
			IsWallJumping = false;
		}

		if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
		{
			_isJumpCut = false;

			if (!IsJumping)
				_isJumpFalling = false;
		}


		if (CanJump() && LastPressedJumpTime > 0)
		{
			IsJumping = true;
			IsWallJumping = false;
			_isJumpCut = false;
			_isJumpFalling = false;
			Jump();
		}

		else if (CanWallJump() && LastPressedJumpTime > 0)
		{
			IsWallJumping = true;
			IsJumping = false;
			_isJumpCut = false;
			_isJumpFalling = false;
			_wallJumpStartTime = Time.time;
			_lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

			WallJump(_lastWallJumpDir);
		}
		#endregion

		#region SLIDE CHECKS
		if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
			IsSliding = true;
		else
			IsSliding = false;
		#endregion

		#region GRAVITY

		if (IsSliding)
		{
			SetGravityScale(0);
		}
		else if (RB.velocity.y < 0 && _moveInput.y < 0)
		{

			SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);

			RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
		}
		else if (_isJumpCut)
		{

			SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
			RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
		}
		else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
		{
			SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
		}
		else if (RB.velocity.y < 0)
		{

			SetGravityScale(Data.gravityScale * Data.fallGravityMult);

			RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
		}
		else
		{

			SetGravityScale(Data.gravityScale);
		}
		#endregion
		//switching goofy ON/OFF
		if(Input.GetKeyDown(KeyCode.Tab))
		{
			switch_goofy();
		}
		//walking animation
		Walking_animated(_moveInput.x);
		//Jumps
		//=============
		//hooked
        if(Input.GetKeyDown(KeyCode.Q) && !goofy)
        {
            Hooked(true);
            Data.jumpForce = Data.jumpForce + heightplus;
            OnJumpInput();
			E_goofy = 0;
			E_ngoofy = 0;
			Q_goofy = 0;
        }
        if (Input.GetKeyUp(KeyCode.Q) && !goofy)
        {
            OnJumpUpInput();
            Data.jumpForce = init_force;
            Hooked(false);
			Q_ngoofy += 1;
        }
        //hammer
        else if (Input.GetKeyDown(KeyCode.Q) && goofy)
		{ 
            Hammer(true);
            Data.jumpForce = Data.jumpForce + heightplus;
            OnJumpInput();
            E_goofy = 0;
            Q_ngoofy = 0;
            E_ngoofy = 0;
        }
        else if (Input.GetKeyUp(KeyCode.Q) && goofy)
        {
            OnJumpUpInput();
            Data.jumpForce = init_force;
            Hammer(false);
			Q_goofy += 1;
        }
		//Dashes
		//==================
		//Decapitation(No head)

		if(Input.GetKeyDown(KeyCode.E) && !goofy)
		{ //
			
			Data.runMaxSpeed=Data.runMaxSpeed+speedplus;
			Axe(true);
			Q_goofy = 0;
			Q_ngoofy = 0;
			E_goofy = 0;
		  }
		if (Input.GetKeyUp(KeyCode.E) && !goofy)
		{
            Data.runMaxSpeed = init_speed;
			Axe(false);
			E_ngoofy += 1;
        }
		//Punch
        if (Input.GetKeyDown(KeyCode.E) && goofy)
        { //
            
            Data.runMaxSpeed = Data.runMaxSpeed + speedplus;
			Punch(true);
            Q_goofy = 0;
            Q_ngoofy = 0;
            E_ngoofy = 0;
        }
        if (Input.GetKeyUp(KeyCode.E) && goofy)
        {
            Data.runMaxSpeed = init_speed;
			Punch(false);
			E_goofy += 1;

        }
		//GOOFY METER FUNCTIONS
		if (E_goofy > 1 || E_ngoofy > 1 || Q_goofy > 1 || Q_ngoofy > 1)
		{
			//Get more sad
		}
    }

	private void FixedUpdate()
	{

		if (IsWallJumping)
			Run(Data.wallJumpRunLerp);
		else
			Run(1);


		if (IsSliding)
			Slide();
	}

    #region INPUT CALLBACKS

    public void OnJumpInput()
	{
		LastPressedJumpTime = Data.jumpInputBufferTime;
	}

	public void OnJumpUpInput()
	{
		if (CanJumpCut() || CanWallJumpCut())
			_isJumpCut = true;
	}
	#endregion

	#region GENERAL METHODS
	public void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}
	#endregion


	#region RUN METHODS
	private void Run(float lerpAmount)
	{

		float targetSpeed = _moveInput.x * Data.runMaxSpeed;

		targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

		#region Calculate AccelRate
		float accelRate;


		if (LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
		#endregion

		#region Add Bonus Jump Apex Acceleration
		
		if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
		{
			accelRate *= Data.jumpHangAccelerationMult;
			targetSpeed *= Data.jumpHangMaxSpeedMult;
		}
		#endregion

		#region Conserve Momentum
		
		if (Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
		{
			accelRate = 0;
		}
		#endregion


		float speedDif = targetSpeed - RB.velocity.x;


		float movement = speedDif * accelRate;

		RB.AddForce(movement * Vector2.right, ForceMode2D.Force);

	}

	private void Turn()
	{
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;

		IsFacingRight = !IsFacingRight;
	}
	#endregion

	#region JUMP METHODS
	private void Jump()
	{
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;

		#region Perform Jump
		float force = Data.jumpForce;
		if (RB.velocity.y < 0)
			force -= RB.velocity.y;

		RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		#endregion
	}

	private void WallJump(int dir)
	{
		
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;
		LastOnWallRightTime = 0;
		LastOnWallLeftTime = 0;

		#region Perform Wall Jump
		Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
		force.x *= dir; 

		if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
			force.x -= RB.velocity.x;

		if (RB.velocity.y < 0) 
			force.y -= RB.velocity.y;


		RB.AddForce(force, ForceMode2D.Impulse);
		#endregion
	}
	#endregion

	#region OTHER MOVEMENT METHODS
	private void Slide()
	{
		float speedDif = Data.slideSpeed - RB.velocity.y;
		float movement = speedDif * Data.slideAccel;		
		movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

		RB.AddForce(movement * Vector2.up);
	}
	#endregion


	#region CHECK METHODS
	public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != IsFacingRight)
			Turn();
	}

	private bool CanJump()
	{
		return LastOnGroundTime > 0 && !IsJumping;
	}

	private bool CanWallJump()
	{
		return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
			 (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
	}

	private bool CanJumpCut()
	{
		return IsJumping && RB.velocity.y > 0;
	}

	private bool CanWallJumpCut()
	{
		return IsWallJumping && RB.velocity.y > 0;
	}

	public bool CanSlide()
	{
		if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && LastOnGroundTime <= 0)
			return true;
		else
			return false;
	}
	#endregion


	#region EDITOR METHODS
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
		Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
	}
	#endregion
	void switch_goofy()
	{
		if (goofy)
		{
			goofy = false;
			goofyswitch.value = 1;
			goofy_text.SetText("SAD");
		}
		else
		{
			goofy = true;
			goofyswitch.value = 0;
            goofy_text.SetText("GOOFY");
        }
	}
	void Walking_animated(float x)
	{
        //walking animation
        if (x != 0)
        {

            animator.SetBool("Walk", true);
        }
        else
        {

            animator.SetBool("Walk", false);
        }

}
	void Hooked(bool x)
	{
		animator.SetBool("hooked", x);

	}
    void Hammer(bool x)
    {
        animator.SetBool("Hammer", x);

    }
    void Axe(bool x)
    {
        animator.SetBool("Head_Off", x);

    }
	void Punch(bool x)
	{ animator.SetBool("Punched", x); }
}