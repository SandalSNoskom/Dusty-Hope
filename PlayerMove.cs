using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove: MonoBehaviour
{
    public Rigidbody2D rb;
	public Animator animator;

	public Transform attackPoint;
	public LayerMask enemyLayers;

	public float attckRange = 0.5f;
	public int attackDamage = 40;

	public VectorValue pos;

	public float AttackRate = 2f;
	float nextAttackTime = 0f;

	void Start()
	{
		transform.position = pos.initialValue;
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		GroundCheckRadius = GroundCheck.GetComponent<CircleCollider2D>().radius;

		realSpeed = speed;
	}

	void Update()
	{
		if(Time.time >= nextAttackTime)
		{
			if (Input.GetKeyDown(KeyCode.L))
			{
				Attack();
				nextAttackTime = Time.time + 1f / AttackRate;
			}
		}

		if (_dashReloaded == true && Input.GetKeyDown(KeyCode.K))
        {
            StartDash();
        }

		Walk();
		Reflect();
		Jump();
		Run();
		// Somersault();
	}


	void FixedUpdate()
	{
		CheckingGround();
		CheckingLadder();
		LaddersMechanics();
		LadderUpDown();
		LADDERS();
		CorrectLadder();
		LadderJump();
	}


	public Vector2 moveVector;
	public int speed = 3;

	public int fastSpeed = 6;
	private int realSpeed;

	void Walk()
	{
		moveVector.x = Input.GetAxisRaw("Horizontal");
		if (!onLadder && !blockMoveXforJump) { rb.velocity = new Vector2(moveVector.x * realSpeed, rb.velocity.y); }
		animator.SetFloat("moveX", Mathf.Abs(moveVector.x));
	}

	public bool faceRight = true;

	void Reflect()
	{
		if (!blockMoveXforJump)
		{
			if ((moveVector.x > 0 && !faceRight) || (moveVector.x < 0 && faceRight))
			{
				Vector3 temp = transform.localScale;
				temp.x *= -1;
				transform.localScale *= new Vector2(-1, 1);
				faceRight = !faceRight;
			}
		}
	}


	public bool onGround;
	public LayerMask Ground;
	public Transform GroundCheck;
	private float GroundCheckRadius;
	void CheckingGround()
	{
		onGround = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius, Ground);
		if ((rb.velocity.y != 0 && bottomCheckedLadder) || blockMoveXforJump) { animator.SetBool("onGround", false); }
		else { animator.SetBool("onGround", onGround); }
	}

	public float jumpForce = 210f;
	private int jumpCount = 0;
	public int maxJumpValue = 1;


	void Jump()
	{
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			Physics2D.IgnoreLayerCollision (9, 10, true);
			Invoke("IgnoreLayerOff", 0.5f);
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (onGround)
			{
				animator.StopPlayback();
				animator.Play("jump");
				rb.velocity = new Vector2(rb.velocity.x, 0);
				rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
			}
			else if (++jumpCount < maxJumpValue)
			{
				animator.StopPlayback();
				animator.Play("doubleJump");
				rb.velocity = new Vector2(0, 10);
			}
		}
		if (onGround) { jumpCount = 0; }

		if (rb.velocity.y == 0) { animator.SetBool("zeroVelocityY", true); }
		else { animator.SetBool("zeroVelocityY", false); }
	}

	void IgnoreLayerOff()
	{
		Physics2D.IgnoreLayerCollision (9, 10, false);
	}

	private bool speedLock;

	void Run()
	{
		if (Input.GetKey(KeyCode.LeftShift) && onGround)
		{
			animator.SetBool("run", true);
			realSpeed = fastSpeed;
			if (Input.GetKeyDown(KeyCode.Space)) { speedLock = true; }
		}
		else if (Input.GetKeyUp(KeyCode.LeftShift) || onGround)
		{
			animator.SetBool("run", false);
			if (!speedLock) { realSpeed = speed; }
			else if (speedLock && onGround) { speedLock = false; }
			else { realSpeed = fastSpeed; }
		}
	}

	// public int SomersaultImpulse = 5000;
	// void Somersault()
	// {
	// 	if (Input.GetKeyDown(KeyCode.K))
	// 	{
	// 		animator.StopPlayback();
	// 		animator.Play("Somersault");

	// 		rb.velocity = new Vector2(0, -1);

	// 		if (rb.transform.localScale.x < 0) { rb.AddForce(Vector2.left * SomersaultImpulse); }
	// 		else { rb.AddForce(Vector2.right * SomersaultImpulse); }
	// 	}
	// }


	[SerializeField]
	private float health = 1f;

	public void TakeDamage(float damage)
	{
		health -= damage;
		if(health <= 0f)
		{
			Die();
		}
	}

	private void Die()
	{
		SceneManager.LoadScene(0);
	}







	//Лестница 
	public float check_RADIUS = 0.04f;
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(CHECK_Ladder.position, check_RADIUS);
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(bottom_Ladder.position, check_RADIUS);
		Gizmos.color = Color.gray;
		//Gizmos.DrawWireSphere(GroundCheck.position, GCRadius);
	}

	public Transform CHECK_Ladder;
	public bool checkedLadder;
	public LayerMask LadderMask;
	public Transform bottom_Ladder;
	public bool bottomCheckedLadder;
	void CheckingLadder()

	{
		checkedLadder = Physics2D.OverlapPoint(CHECK_Ladder.position, LadderMask);
		bottomCheckedLadder = Physics2D.OverlapPoint(bottom_Ladder.position, LadderMask);
	}

	public float ladderSpeed = 1.5f;
	void LaddersMechanics()
	{
		if (onLadder)
		{
			rb.bodyType = RigidbodyType2D.Kinematic;
			rb.velocity = new Vector2(rb.velocity.x, moveVector.y * ladderSpeed);
		}
		else { rb.bodyType = RigidbodyType2D.Dynamic; }
	}

	void LadderUpDown()
	{
		moveVector.y = Input.GetAxisRaw("Vertical");
		animator.SetFloat("moveY", moveVector.y);
		animator.SetInteger("LadderUpDown", (int)moveVector.y);
	}
	float vertInput;

	public bool onLadder;
	void LADDERS()
	{
		vertInput = Input.GetAxisRaw("Vertical");
		if (checkedLadder || bottomCheckedLadder)
		{
			if (!checkedLadder && bottomCheckedLadder) // ПЕРС СВЕРХУ
			{
				if (vertInput > 0) { onLadder = false; animator.Play("ladder IDLE"); }
				else if (vertInput < 0) { onLadder = true; }
			}
			else if (checkedLadder && bottomCheckedLadder) // НА ЛЕСТНИЦЕ
			{
				if (vertInput > 0) { onLadder = true; }
				else if (vertInput < 0) { onLadder = true; }
			}
			else if (checkedLadder && !bottomCheckedLadder) // ПЕРС СНИЗУ
			{
				if (vertInput > 0) { onLadder = true; }
				else if (vertInput < 0) { onLadder = false; }
			}
		}
		else { onLadder = false; } // ВНЕ ЛЕСТНИЦЫ

		LaddersMechanics();

		animator.SetBool("onLadder", onLadder);
	}
	bool corrected = true;
	void CorrectLadder()

	{
		if (onLadder && corrected) { corrected = !corrected; rb.velocity = Vector2.zero; LadderCenter(); }
		else if (!onLadder && !corrected) { corrected = true; }
	}
	float ladderCenter;
	void LadderCenter()
	{
		ladderCenter = Physics2D.OverlapPoint(CHECK_Ladder.position, LadderMask).gameObject.transform.position.x; Debug.Log(ladderCenter);
		if (checkedLadder) { ladderCenter = Physics2D.OverlapPoint(CHECK_Ladder.position, LadderMask).GetComponent<BoxCollider2D> ().bounds.center.x; }
		else if (bottomCheckedLadder) { ladderCenter = Physics2D.OverlapPoint(bottom_Ladder.position, LadderMask).GetComponent<BoxCollider2D>().bounds.center.x; }
		transform.position = new Vector2(ladderCenter, transform.position.y);
	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "LadderStairs") { Physics2D.IgnoreCollision(collision.GetComponent<EdgeCollider2D>(),
		GetComponent<CapsuleCollider2D>(), true); }
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == "LadderStairs") { Physics2D.IgnoreCollision(collision.GetComponent<EdgeCollider2D>(),
		GetComponent<CapsuleCollider2D>(), false); }
	}
	private bool blockMoveXforJump;

	public float jumpLadderTime = 0.5f;
	private float timerJumpLadder;
	public Vector2 jumpAngle = new Vector2(3.5f, 5);
	void LadderJump()
	{
		if (onLadder && Mathf.Abs(moveVector.x) > 0 && Input.GetKeyDown(KeyCode.Space) && rb.velocity.y == 0)
		{
			blockMoveXforJump = true;
			moveVector.x = 0;
			onLadder = false; ////////////////////////////// Очень важно для корректной анимации!
			animator.StopPlayback();
			animator.Play("jump");
			LaddersMechanics();
			rb.velocity = new Vector2(0, 0);
			rb.velocity = new Vector2(transform.localScale.x * jumpAngle.x, jumpAngle.y);
		}
		else if (onLadder && Input.GetKeyDown(KeyCode.Space) && rb.velocity.y == 0) { onLadder = false; }

		if (blockMoveXforJump)
		{
			if ((timerJumpLadder += Time.deltaTime) >= jumpLadderTime)
			{
				if (onLadder || onGround || Input.GetAxisRaw("Horizontal") != 0)
				{
					blockMoveXforJump = false;
					timerJumpLadder = 0;
				}
			}
		}
	}

	////////////АТАКА

	void Attack()
	{
		///// Анимация атаки
		animator.SetTrigger("Attack");

		///// Обнаружить всех врагов, которые в зоне атаки
		Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attckRange, enemyLayers);

		///// Наносить урон врагам
		foreach(Collider2D enemy in hitEnemies)
		{
			enemy.GetComponent<Zombie1>().TakeDamage(attackDamage);
		}
	}

	void OnDrawGizmosSelected()
	{
		if (attackPoint == null)
			return;

		Gizmos.DrawWireSphere(attackPoint.position, attckRange);
	}

	[SerializeField] public bool _onDash = false;
	[SerializeField] public bool _startDash = false;
	[SerializeField] public bool _dashReloaded = true;
	//private bool _onDashCollision = false;
	[SerializeField] private Vector2 _inputVector = new Vector2(0, 0);

	[SerializeField] private float _dashDistance = 3f;
	[SerializeField] private float _dashSpeed = 0.1f;
	[SerializeField] private float _dashTimeReload = 2f;
	private Vector2 _dashFinishPosition;
	private Vector2 _dashCurrentPosition;
	[SerializeField] private float _dashProgress = 0f;


	private void StartDash()
    {
        _dashCurrentPosition = transform.position;
        _dashFinishPosition = _dashCurrentPosition + _dashDistance * Vector2.right * (faceRight ? 1 : -1); // faceRight - это из метода отражения
        _dashProgress = 0f;
        _onDash = true;
        _dashReloaded = false;

        //rb.gravityScale = 0;
        rb.velocity = Vector2.zero; // rb - это ссылка на Rigidbody2D
        StartCoroutine(DashReloader());
    }

    private void Dash()
    {
        _dashProgress += Time.fixedDeltaTime * _dashSpeed / _dashDistance;

        if (_dashProgress <= 1f)
        {
            _dashCurrentPosition = Vector2.MoveTowards(transform.position, _dashFinishPosition, Time.fixedDeltaTime * _dashSpeed);
            rb.MovePosition(_dashCurrentPosition);
            // rb.velocity = Vector2.zero; // убирает все паразитные скорости, можешь попробовать отключить для кувырка
            animator.Play("dash", 0, _dashProgress); // anim - ссылка на Аниматор, а вместо "dash" название твоей анимации кувырка
        }
        else
        {
            _onDash = false; // этой переменной можно будет блокировать управление на время кувырка
            //rb.gravityScale = gravityDef;
            rb.velocity = Vector2.zero;
            //anim.Play("idle"); // можно убрать эту строку, если настроены переходы в Аниматоре после кувырка
        }
    }


    IEnumerator DashReloader()
    {
        yield return new WaitForSeconds(_dashTimeReload);
        _dashReloaded = true;
    }
}




